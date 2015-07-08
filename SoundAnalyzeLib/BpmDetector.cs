using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams;

namespace SoundAnalyzeLib
{
    /// <summary>
    /// 楽曲ファイルからテンポ（BPM)を検出するクラス
    /// </summary>
    public class BpmDetector : IBpmDetector
    {
        /// <summary>ファイル名</summary>
        private string _fileName;
        /// <summary>ファイル名</summary>
        public string FileName { get { return _fileName; } }

        public BPMDetectorConfig _config;
        public BPMDetectorConfig Config { get { return _config; } }

        /// <summary>各BPMでの一致度リスト</summary>
        private List<KeyValuePair<int, double>> _bpm;
        /// <summary>各BPMでの一致度リスト</summary>
        //        public List<KeyValuePair<int, double>> BPM { get { return _bpm; } }
        public Dictionary<int, double> BPMs
        {
            get
            {
                return _bpm
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }
        /// <summary>BPMのピークリスト</summary>
        private List<KeyValuePair<int, double>> _peaks;
        /// <summary>BPMのピークリスト</summary>
        //public List<KeyValuePair<int, double>> Peaks { get { return _peaks; } }
        public Dictionary<int, double> Peaks
        {
            get
            {
                return _peaks
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }

        /// <summary>閾値を超えたピーク値を持つBPMのリスト</summary>
        private List<KeyValuePair<int, double>> _topPeaks;
        /// <summary>閾値を超えたピーク値を持つBPMのリスト</summary>
        //public List<KeyValuePair<int, double>> TopPeaks { get { return _topPeaks; } }
        public Dictionary<int, double> TopPeaks
        {
            get
            {
                return _topPeaks
                    .ToDictionary(x => x.Key, x => x.Value);
            }
        }

        /// <summary>テンポ計算する際に使用する、音量計算に使用するサンプル数</summary>
        int _frameSize = 512;
        /// <summary>テンポ計算する際に使用する、音量計算に使用するサンプル数</summary>
        public int FrameSize { get { return _frameSize; } }

        int _bpmLow = 60;
        /// <summary>
        /// 検出するテンポの下限
        /// </summary>
        public int BpmLow { get { return _bpmLow; } }

        int _bpmHigh = 220;
        /// <summary>
        /// 検出するテンポの上限
        /// </summary>
        public int BpmHigh { get { return _bpmHigh; } }

        int _priorityBpmLow = 110;
        /// <summary>
        /// 優先するテンポの小さい側
        /// </summary>
        public int BriorityBpmLow { get { return _priorityBpmLow; } }

        int _priorityBpmHigh = 150;
        /// <summary>
        /// 優先するテンポの大きい側
        /// </summary>
        public int BriorityBpmHigh { get { return _priorityBpmHigh; } }

        double _peakThreshold = 0.75;
        /// <summary>
        /// 優先するテンポ計算に使用する閾値
        /// </summary>
        public double PeakThreshold { get { return _peakThreshold; } }

        int _peakWidth = 3;
        /// <summary>
        /// ピーク計算に使用するデータ幅
        /// </summary>
        public int PeakWidth { get { return _peakWidth; } }

        public int BPM { get; set; }
        public int ID { get; set; }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="config"></param>
        public BpmDetector(BPMDetectorConfig config)
        {
            _frameSize = config.FrameSize;
            _bpmLow = config.BPMLow;
            _bpmHigh = config.BPMHigh;
            _priorityBpmLow = config.PriorityBPMLow;
            _priorityBpmHigh = config.PriorityBPMHigh;
            _peakThreshold = config.PeakThreshold;
            _peakWidth = config.PeakWidth;
        }


        /// <summary>
        /// BPM検出
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int detect(String fileName)
        {
            _fileName = fileName;
            IWaveSource waveSource = CodecFactory.Instance.GetCodec(fileName);
            ISampleSource sampleSource = waveSource.ToSampleSource();
            List<double> diff = getDiff(sampleSource);
            calcBpm(diff, sampleSource.WaveFormat.SampleRate);
            findPeaks(_peakWidth);
            return getBpm();
        }

        public int getBpm()
        {
            double peakMax = 0;
            int bMax = 0;
            foreach (KeyValuePair<int, double> bpm in _topPeaks)
            {
                double p = bpm.Value;

                if (peakMax == 0)
                {
                    peakMax = p;
                    bMax = bpm.Key;
                }
                else if (_priorityBpmLow <= bpm.Key
                    && bpm.Key < _priorityBpmHigh
                    && (bMax % bpm.Key == 0 || bpm.Key % bMax == 0))
                {
                    bMax = bpm.Key;
                }
            }
            return bMax;
        }

        /// <summary>
        /// 音量差分リストを求める
        /// </summary>
        /// <param name="sampleSource"></param>
        /// <returns></returns>
        List<double> getDiff(ISampleSource sampleSource)
        {
            List<double> diff = new List<double>();
            int ch = sampleSource.WaveFormat.Channels;
            int bufferSize = _frameSize * ch;
            double p_vol = 0;
            int read;
            float[] buff = new float[bufferSize];
            bool bStarted = false;
            while ((read = sampleSource.Read(buff, 0, bufferSize)) == bufferSize)
            {
                double v = 0;
                for (int i = 0; i < bufferSize; i += ch)
                {
                    v += Math.Pow(buff[i], 2); ;
                    if (ch > 1)
                    {
                        v += Math.Pow(buff[i + 1], 2);
                    }
                }
                if (!bStarted && v > 0) { bStarted = true; }

                if (bStarted)
                {
                    double vol = Math.Sqrt(v);
                    if (vol > p_vol)
                    {
                        diff.Add(vol - p_vol);
                    }
                    else
                    {
                        diff.Add(0);
                    }
                    p_vol = vol;
                }
            }
            for (int i = diff.Count - 1; i > 0; i--)
            {
                if (diff[i] == 0)
                {
                    diff.RemoveAt(i);
                }
            }
            return diff;
        }


        /// <summary>
        /// BPM成分抽出
        /// </summary>
        public void calcBpm(List<double> diff, int sampleRate)
        {
            double s = (double)sampleRate / (double)_frameSize;
            _bpm = new List<KeyValuePair<int, double>>();
            for (int bpm = _bpmLow; bpm <= _bpmHigh; bpm++)
            {
                double a_sum = 0;
                double b_sum = 0;
                double f = (double)bpm / 60;
                for (int n = 0; n < diff.Count; n++)
                {
                    double ham = diff[n] * 0.54 - 0.46 * Math.Cos(2 * Math.PI * (double)n / ((double)diff.Count - 1));
                    a_sum += ham * Math.Cos(2.0 * Math.PI * f * (double)n / s);
                    b_sum += ham * Math.Sin(2.0 * Math.PI * f * (double)n / s);
                }
                double a_tmp = a_sum / (double)diff.Count;
                double b_tmp = b_sum / (double)diff.Count;
                _bpm.Add(new KeyValuePair<int, double>(bpm, Math.Sqrt(Math.Pow(a_tmp, 2) + Math.Pow(b_tmp, 2))));
            }
        }
        public List<double> setHammingWindow(List<double> list)
        {
            return list.Select((x, i) => 0.54 - 0.46 * Math.Cos(2 * Math.PI * (double)i / ((double)list.Count - 1))).ToList();
        }
        public Dictionary<int, double> calculateBPM(Dictionary<double, double> diff, int sampleRate, int frameSize, int bpmLow = 60, int bpmHigh = 240)
        {
            double s = (double)sampleRate / (double)frameSize;
            Dictionary<int, double> bpmList = new Dictionary<int, double>();
            for (int bpm = bpmLow; bpm <= bpmHigh; bpm++)
            {
                double f = (double)bpm / 60;
                double a_sum = diff.Select((x, i) => Math.Cos(2.0 * Math.PI * f * x.Value * (double)i / s)).Sum();
                double b_sum = diff.Select((x, i) => Math.Sin(2.0 * Math.PI * f * x.Value * (double)i / s)).Sum();
                bpmList.Add(bpm, Math.Sqrt(Math.Pow(a_sum / (double)diff.Count, 2) + Math.Pow(b_sum / (double)diff.Count, 2)));
            }
            return bpmList;
        }
        /// <summary>
        /// BPMピークを求め、ピーク値の大きい順にソートする
        /// </summary>
        /// <param name="width">ピーク判定に使用するデータ数。前後にwidth分を比較し、それらより大きい場合にピークと判定する</param>
        /// <returns>検出したピーク数</returns>
        public int findPeaks(int width)
        {
            _peaks = new List<KeyValuePair<int, double>>();
            for (int i = 0; i < _bpm.Count; i++)
            {
                bool isPeak = true;
                for (int j = 1; j <= width; j++)
                {
                    double val = 0;
                    //before
                    if (i - j < 0)
                    {
                        val = 0;
                    }
                    else
                    {
                        val = _bpm[i - j].Value;
                    }
                    if (val > _bpm[i].Value)
                    {
                        isPeak = false;
                        break;
                    }
                    //after
                    if (i + j < _bpm.Count)
                    {
                        val = _bpm[i + j].Value;
                    }
                    else
                    {
                        val = 0;
                    }
                    if (val > _bpm[i].Value)
                    {
                        isPeak = false;
                        break;
                    }
                }
                if (isPeak)
                {
                    _peaks.Add(_bpm[i]);
                }
            }
            _peaks.Sort(delegate(KeyValuePair<int, double> x, KeyValuePair<int, double> y)
            {
                double d = y.Value - x.Value;// _bpm[y] - _bpm[x];
                if (d == 0) { return 0; }
                else if (d > 0) { return 1; }
                else return -1;
            });

            double max = _peaks[0].Value;
            _topPeaks = new List<KeyValuePair<int, double>>();
            foreach (KeyValuePair<int, double> pk in _peaks)
            {
                if (pk.Value / max > PeakThreshold)
                {
                    _topPeaks.Add(pk);
                }
            }

            return _peaks.Count;
        }


        public void loadFromFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void saveToFile(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
