using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams;

namespace SoundAnalyzeLib
{
    /// <summary>
    /// 音量の自己相関によるBPM検出クラス
    /// </summary>
    public class BPMVolumeAutoCorrelation : IBpmDetector
    {
        #region Properties
        /// <summary>
        /// BPM検出に使用する各種設定値格納用
        /// </summary>
        BPMDetectorConfig _config;
        public BPMDetectorConfig Config { get { return _config; } }

        /// <summary>
        /// 分析する楽曲ファイル名
        /// </summary>
        string _fileName;
        public string FileName { get { return _fileName; } }

        /// <summary>
        /// BPM毎の一致度を格納するリスト。
        /// </summary>
        Dictionary<int, double> _bpms;
        public Dictionary<int, double> BPMs { get { return _bpms; } }

        /// <summary>
        /// ピーク検出したBPM一致度リスト。BPMの候補ね。
        /// </summary>
        Dictionary<int, double> _peaks;
        public Dictionary<int, double> Peaks { get { return _peaks; } }

        /// <summary>
        /// ピーク検出したものの内、閾値を超えたもののリスト
        /// </summary>
        Dictionary<int, double> _topPeaks;
        public Dictionary<int, double> TopPeaks { get { return _topPeaks; } }

        /// <summary>
        /// 自己相関格納リスト
        /// </summary>
        Dictionary<double, double> _autoCorrelation;
        public Dictionary<double, double> AutoCorrelation { get { return _autoCorrelation; } }

        /// <summary>
        /// 一次近似y=ax+bのａ
        /// </summary>
        double _a;
        public double A { get { return _a; } }

        /// <summary>
        /// 一次近似y=ax+bのb
        /// </summary>
        double _b;
        public double B { get { return _b; } }

        /// <summary>
        /// 自己相関を一次近似直線により正規化したもの
        /// </summary>
        Dictionary<double, double> _normalized;
        public Dictionary<double, double> Normalized { get { return _normalized; } }

        /// <summary>
        /// 一次近似直線
        /// </summary>
        public Dictionary<double, double> Liner
        {
            get
            {
                if (_autoCorrelation != null)
                {
                    return _autoCorrelation.ToDictionary(x => x.Key, x => _a * x.Key + _b);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 検出したBPM
        /// </summary>
        public int BPM { get; set; }

        /// <summary>
        /// iTunesのDatabaseIDを保持
        /// </summary>
        public int ID { get; set; }

        #endregion

        #region Const

        /// <summary>
        /// 2Π
        /// </summary>
        const double _2Pi = 2.0 * Math.PI;

        #endregion

        #region constructor

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="config"></param>
        public BPMVolumeAutoCorrelation(BPMDetectorConfig config)
        {
            _config = config;
            BPM = 0;
        }
        public BPMVolumeAutoCorrelation()
        {
            _config = new BPMDetectorConfig();
            BPM = 0;
        }

        #endregion

        #region private method

        /// <summary>
        /// フレーム毎に振幅の二乗和を計算しその平方根をリスト化する
        /// </summary>
        /// <remarks>
        /// nフレーム目の音量は<br/>
        /// V(n)=√(ΣD(i))^2
        /// </remarks>
        /// <param name="sampleSource"></param>
        /// <param name="frameSize"></param>
        /// <returns></returns>
        Dictionary<double, double> getVolume(ISampleSource sampleSource, int frameSize)
        {
            int channels = sampleSource.WaveFormat.Channels;
            //フレームごとの読み込みサンプル数（フレームサイズ×チャネル数）
            int readSize = frameSize * channels;
            //バッファ
            float[] buffer = new float[readSize];

            //音量情報を格納するDictionary(時刻、音量)
            Dictionary<double, double> volume = new Dictionary<double, double>();

            //フレームサイズ×チャネル数分ずつ読み込み、音量を検出する
            int frameCounter = 0;
            double pos = sampleSource.GetPosition().TotalSeconds;
            while (sampleSource.Read(buffer, 0, readSize) == readSize)
            {
                double vol = 0;
                for (int ch = 0; ch < sampleSource.WaveFormat.Channels; ch++)
                {
                    float[] volChannel = buffer.Where((v, i) => i % channels == ch).ToArray();
                    vol += Math.Sqrt(volChannel.Zip(volChannel, (i, j) => i * j).Sum() / (double)frameSize);
                }
                volume.Add(pos, vol);
                frameCounter++;
                pos = sampleSource.GetPosition().TotalSeconds;
            }
            return volume;
        }

        /// <summary>
        /// 音量データの自己相関を求める
        /// </summary>
        /// <param name="data"></param>
        /// <param name="correlationSize"></param>
        /// <returns></returns>
        Dictionary<double, double> getAutoCorrelation(Dictionary<double, double> data, int correlationSize)
        {
            Dictionary<double, double> autoCorrelation = new Dictionary<double, double>();
            double norm = data.Zip(data, (i, j) => i.Value * j.Value).Sum() / (double)data.Count;
            for (int n = 0; n < correlationSize; n++)
            {
                double inp = data.Zip(data.Skip(n), (i, j) => i.Value * j.Value).Sum() / (double)(data.Count - n);
                autoCorrelation.Add(data.ElementAt(n).Key, inp / norm);
            }
            return autoCorrelation;
        }

        /// <summary>
        /// 一次近似直線<br/>
        /// y=ax+b<br/>
        /// のaとbを算出する
        /// </summary>
        /// <remarks>
        /// P=Σx,Q=Σｙ,R=Σ(xy),S=Σ(x)^2とすると<br/>
        /// a=(nR-PQ)/(nS-PR)<br/>
        /// b-(SQ-RP)/(nS-P^2)
        /// </remarks>
        /// <param name="data"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        void getLinerParam(Dictionary<double, double> data, out double a, out double b)
        {
            double n = (double)data.Count;
            //
            double P = data.Sum(d => d.Key);
            double Q = data.Sum(d => d.Value);
            double R = data.Sum(d => d.Key * d.Value);
            double S = data.Sum(d => d.Key * d.Key);

            a = (n * R - P * Q) / (n * S - P * P);
            b = (S * Q - R * P) / (n * S - P * P);
        }

        public Dictionary<int, double> getBPM(Dictionary<double, double> data)
        {
            Dictionary<int, double> bpmList = new Dictionary<int, double>();
            for (int bpm = _config.BPMLow; bpm <= _config.BPMHigh; bpm++)
            {
                double f = (double)bpm / 60;
                double a_sum = data
                    .Select((x, i) => x.Value * bpmFuncCos2(x.Key, f))
                    .Sum();
                bpmList.Add(bpm, a_sum);
            }
            double m = bpmList.Max(x => x.Value);
            return bpmList.ToDictionary(x => x.Key, x => x.Value / m);
        }

        double bpmFuncCos(double t, double f)
        {
            return Math.Cos(_2Pi * f * t);
        }
        double bpmFuncCos2(double t, double f)
        {
            return 0.4 * Math.Cos(_2Pi * f * t) + 0.6 * Math.Cos(_2Pi * f * t * 2);
        }

        int selectPeak(Dictionary<int, double> topPeaks)
        {
            int firstBPM = topPeaks.First().Key;
            foreach (KeyValuePair<int, double> p in topPeaks)
            {
                if (p.Key >= _config.PriorityBPMLow && _config.PriorityBPMHigh >= p.Key)
                {
                    return p.Key;
                    //        if (p.Key % firstBPM < 2 || firstBPM % p.Key < 2)
                    //        {
                    //            return p.Key;
                    //        }
                    //        if (p.Key % (firstBPM - 1) < 2 || firstBPM % (p.Key - 1) < 2)
                    //        {
                    //            return p.Key;
                    //        }
                    //        if (p.Key % (firstBPM + 1) < 2 || firstBPM % (p.Key + 1) < 2)
                    //        {
                    //            return p.Key;
                    //        }
                }
            }
            return firstBPM;
        }

        #endregion

        void writeDictionary(Dictionary<double, double> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<double, double> kv in dictionary)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }
        }
        void writeDictionary(Dictionary<int, double> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, double> kv in dictionary)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }
        }
        void readDictionary(Dictionary<double, double> dictionary, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                double key = reader.ReadDouble();
                double value = reader.ReadDouble();
                dictionary.Add(key, value);
            }
        }
        void readDictionary(Dictionary<int, double> dictionary, BinaryReader reader)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int key = reader.ReadInt32();
                double value = reader.ReadDouble();
                dictionary.Add(key, value);
            }
        }
        public void saveToFile(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(_config.FrameSize);
                    writer.Write(_config.AutoCorrelationSize);
                    writer.Write(_config.BPMHigh);
                    writer.Write(_config.BPMLow);
                    writer.Write(_config.PriorityBPMHigh);
                    writer.Write(_config.PriorityBPMLow);
                    writer.Write(_config.PeakThreshold);
                    writer.Write(_config.PeakWidth);

                    writeDictionary(_autoCorrelation, writer);
                    writer.Write(_a);
                    writer.Write(_b);
                    writeDictionary(_normalized, writer);
                    writeDictionary(_peaks, writer);
                    writeDictionary(_topPeaks, writer);
                    writer.Write(BPM);


                    writer.Close();
                }
                stream.Close();
            }
        }

        public int loadFromFile(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    _config = new BPMDetectorConfig();
                    _config.FrameSize = reader.ReadInt32();

                    _config.AutoCorrelationSize = reader.ReadInt32();
                    _config.BPMHigh = reader.ReadInt32();
                    _config.BPMLow = reader.ReadInt32();
                    _config.PriorityBPMHigh = reader.ReadInt32();
                    _config.PriorityBPMLow = reader.ReadInt32();
                    _config.PeakThreshold = reader.ReadDouble();
                    _config.PeakWidth = reader.ReadInt32();

                    _autoCorrelation = new Dictionary<double, double>();
                    readDictionary(_autoCorrelation, reader);
                    _a = reader.ReadDouble();
                    _b = reader.ReadDouble();
                    _normalized = new Dictionary<double, double>();
                    readDictionary(_normalized, reader);
                    _peaks = new Dictionary<int, double>();
                    readDictionary(_peaks, reader);
                    _topPeaks = new Dictionary<int, double>();
                    readDictionary(_topPeaks, reader);
                    BPM = reader.ReadInt32();

                    reader.Close();
                }
                stream.Close();
            } return detectFromAutoCorrelation();
        }

        int detectFromAutoCorrelation()
        {
            //BPM検出
            _bpms = getBPM(_normalized);

            //ピーク検出
            _peaks = _bpms.Skip(1)
                .Take(_bpms.Count - 2)
                .Where(x => _bpms[x.Key - 1] < x.Value && _bpms[x.Key + 1] < x.Value)
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            //しきい値以上のピークに絞り込む
            double max = _peaks.Max(x => x.Value);
            _topPeaks = _peaks
                .Where(x => x.Value / max > _config.PeakThreshold)
                .ToDictionary(x => x.Key, x => x.Value);

            BPM = selectPeak(_topPeaks);
            return BPM;
        }

        public int detect(string fileName)
        {
            _fileName = fileName;
            IWaveSource waveSource = CodecFactory.Instance.GetCodec(fileName);
            ISampleSource sampleSource = waveSource.ToSampleSource();

            //音量情報を格納するDictionary(時刻、音量)
            Dictionary<double, double> volume = getVolume(sampleSource, _config.FrameSize);

            //自己相関を求める
            _autoCorrelation = getAutoCorrelation(volume, _config.AutoCorrelationSize);

            //最小二乗法により一次近似直線(y=ax+b)のパラメータa,bを求める
            //            double a, b;
            getLinerParam(_autoCorrelation, out _a, out _b);
            //一次近似直線により正規化
            _normalized = _autoCorrelation
                .ToDictionary(x => x.Key, x => x.Value - (_a * x.Key + _b));

            return detectFromAutoCorrelation();

            /*
            //BPM検出
            _bpms = getBPM(_normalized);

            //ピーク検出
            _peaks = _bpms.Skip(1)
                .Take(_bpms.Count - 2)
                .Where(x => _bpms[x.Key - 1] < x.Value && _bpms[x.Key + 1] < x.Value)
                .OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            //しきい値以上のピークに絞り込む
            double max = _peaks.Max(x => x.Value);
            _topPeaks = _peaks
                .Where(x => x.Value / max > _config.PeakThreshold)
                .ToDictionary(x => x.Key, x => x.Value);

            BPM = selectPeak(_topPeaks);
            return BPM;
            */
        }
    }
}
