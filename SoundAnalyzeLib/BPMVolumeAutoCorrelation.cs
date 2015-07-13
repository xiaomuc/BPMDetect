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
    public delegate double BeatWave(double t, double v);

    /// <summary>
    /// 音量の自己相関によるBPM検出クラス
    /// </summary>
    public class BPMVolumeAutoCorrelation : IBpmDetector
    {
        #region WaveFunctions
        /// <summary>
        /// 単純コサイン関数
        /// </summary>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double waveCos(double t, double f)
        {
            return Math.Cos(_2Pi * f * t);
        }

        /// <summary>
        /// 8ビート系
        /// </summary>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double wave8beat(double t, double f)
        {
            return 0.4 * Math.Cos(_2Pi * f * t) + 0.6 * Math.Cos(_2Pi * f * t * 2);
        }
        /// <summary>
        /// 三拍子系
        /// </summary>
        /// <param name="t"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static double wave3note(double t, double f)
        {
            return 0.6 * Math.Cos(_2Pi * f * t) + 0.4 * Math.Cos(_2Pi * f * t / 3);

        }
        /// <summary>
        /// BPM特定に使用する波(COS波)関数
        /// </summary>
        BeatWave beatWave;
        #endregion

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
        public BPMVolumeAutoCorrelation(BPMDetectorConfig config, BeatWave bw)
        {
            _config = config;
            beatWave = bw;
            BPM = 0;
        }
        public BPMVolumeAutoCorrelation(BPMDetectorConfig config)
            : this(config, wave8beat)
        {
        }
        public BPMVolumeAutoCorrelation()
            : this(new BPMDetectorConfig(), wave8beat)
        {
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
        Dictionary<double, double> getVolume(string fileName, int frameSize)
        {
            IWaveSource waveSource = CodecFactory.Instance.GetCodec(fileName);
            ISampleSource sampleSource = waveSource.ToSampleSource();

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
            waveSource.Dispose();
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
            if (correlationSize > data.Count)
            {
                correlationSize = data.Count;
            }
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

        /// <summary>
        /// BPMマッチ度を計算
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Dictionary<int, double> getBPM(Dictionary<double, double> data)
        {
            Dictionary<int, double> bpmList = new Dictionary<int, double>();
            for (int bpm = _config.BPMLow; bpm <= _config.BPMHigh; bpm++)
            {
                double f = (double)bpm / 60;
                double a_sum = data
                    .Select((x, i) => x.Value * beatWave(x.Key, f))
                    .Sum();
                bpmList.Add(bpm, a_sum);
            }
            double m = bpmList.Max(x => x.Value);
            return bpmList.ToDictionary(x => x.Key, x => x.Value / m);
        }

        /// <summary>
        /// 閾値を超えているピークから、優先BPM範囲内のものを抽出する
        /// </summary>
        /// <param name="topPeaks"></param>
        /// <returns></returns>
        int selectPeak(Dictionary<int, double> topPeaks)
        {
            int firstBPM = topPeaks.First().Key;
            foreach (KeyValuePair<int, double> p in topPeaks)
            {
                if (p.Key >= _config.PriorityBPMLow && _config.PriorityBPMHigh >= p.Key)
                {
                    return p.Key;
                }
            }
            return firstBPM;
        }

        /// <summary>
        /// double,doubleのディクショナリをファイルに書き込む
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="writer"></param>
        void writeDictionary(Dictionary<double, double> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<double, double> kv in dictionary)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }
        }

        /// <summary>
        /// int,doubleのディクショナリをファイルに書き込む
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="writer"></param>
        void writeDictionary(Dictionary<int, double> dictionary, BinaryWriter writer)
        {
            writer.Write(dictionary.Count);
            foreach (KeyValuePair<int, double> kv in dictionary)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }
        }

        /// <summary>
        /// double,doubleのディクショナリをファイルから読み込む
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="reader"></param>
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

        /// <summary>
        /// int,doubleのディクショナリをファイルから読み込む
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="reader"></param>
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
        #endregion

        #region public method

        /// <summary>
        /// ファイル保存処理
        /// </summary>
        /// <param name="fileName"></param>
        public void saveToFile(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    //Configの保存
                    writer.Write(_config.FrameSize);
                    writer.Write(_config.AutoCorrelationSize);
                    writer.Write(_config.BPMHigh);
                    writer.Write(_config.BPMLow);
                    writer.Write(_config.PriorityBPMHigh);
                    writer.Write(_config.PriorityBPMLow);
                    writer.Write(_config.PeakThreshold);
                    writer.Write(_config.PeakWidth);

                    //自己相関
                    writeDictionary(_autoCorrelation, writer);

                    //一時近似係数
                    writer.Write(_a);
                    writer.Write(_b);

                    //一時近似で正規化後の自己相関
                    writeDictionary(_normalized, writer);

                    //BPM
                    writeDictionary(_bpms, writer);

                    //ピーク
                    writeDictionary(_peaks, writer);

                    //閾値を超えたピーク
                    writeDictionary(_topPeaks, writer);

                    //特定したBPM
                    writer.Write(BPM);

                    writer.Close();
                }
                stream.Close();
            }
        }

        /// <summary>
        /// ファイルからの読み込み処理
        /// </summary>
        /// <param name="fileName"></param>
        public void loadFromFile(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    //Config
                    _config = new BPMDetectorConfig();
                    _config.FrameSize = reader.ReadInt32();
                    _config.AutoCorrelationSize = reader.ReadInt32();
                    _config.BPMHigh = reader.ReadInt32();
                    _config.BPMLow = reader.ReadInt32();
                    _config.PriorityBPMHigh = reader.ReadInt32();
                    _config.PriorityBPMLow = reader.ReadInt32();
                    _config.PeakThreshold = reader.ReadDouble();
                    _config.PeakWidth = reader.ReadInt32();

                    //自己相関
                    _autoCorrelation = new Dictionary<double, double>();
                    readDictionary(_autoCorrelation, reader);

                    //一時近似係数
                    _a = reader.ReadDouble();
                    _b = reader.ReadDouble();

                    //一時近似で正規化後の自己相関
                    _normalized = new Dictionary<double, double>();
                    readDictionary(_normalized, reader);

                    //BPM
                    _bpms = new Dictionary<int, double>();
                    readDictionary(_bpms, reader);

                    //ピーク
                    _peaks = new Dictionary<int, double>();
                    readDictionary(_peaks, reader);

                    //閾値を超えたピーク
                    _topPeaks = new Dictionary<int, double>();
                    readDictionary(_topPeaks, reader);

                    //特定したBPM
                    BPM = reader.ReadInt32();

                    reader.Close();
                }
                stream.Close();
            }
        }

        /// <summary>
        /// BPM検出
        /// </summary>
        /// <remarks>
        /// <list type="number">
        /// <item><description>音量検出</description></item>
        /// <item><description>自己相関</description></item>
        /// <item><description>自己相関の一時近似係数</description></item>
        /// <item><description>一時近似直線で自己相関を正規化</description></item>
        /// <item><description>BPMマッチ度計算</description></item>
        /// <item><description>BPMピーク検出</description></item>
        /// <item><description>閾値を超えたピークの検出</description></item>
        /// <item><description>優先度を考慮したBPMを特定</description></item>
        /// </list>
        /// </remarks>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int detect(string fileName)
        {
            _fileName = fileName;

            //音量情報を格納するDictionary(時刻、音量)
            Dictionary<double, double> volume = getVolume(fileName, _config.FrameSize);

            //自己相関を求める
            _autoCorrelation = getAutoCorrelation(volume, _config.AutoCorrelationSize);

            //最小二乗法により一次近似直線(y=ax+b)のパラメータa,bを求める
            getLinerParam(_autoCorrelation, out _a, out _b);

            //一次近似直線により正規化
            _normalized = _autoCorrelation
                .ToDictionary(x => x.Key, x => x.Value - (_a * x.Key + _b));

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

            //優先度を考慮してBPMを検出する
            BPM = selectPeak(_topPeaks);

            return BPM;
        }
        #endregion
    }
}
