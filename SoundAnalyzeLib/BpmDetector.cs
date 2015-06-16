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
    public class BpmDetector
    {
        //fields
        private List<double> _bpm;

        //properties
        private string _fileName;
        public string FileName { get { return _fileName; } }

        private IWaveSource _source;
        public IWaveSource WaveSource { get { return _source; } }

        private ISampleSource _sampleSource;
        public ISampleSource SampleSource { get { return _sampleSource; } }

        private List<double> _volume;
        public List<double> Volume { get { return _volume; } }

        private List<double> _diff;
        public List<double> Diff { get { return _diff; } }

        private List<int> _peaks;
        public List<int> Peaks { get { return _peaks; } }

        int _frameSize = 512;
        /// <summary>
        /// テンポ計算する際に使用する、音量計算に使用するサンプル数
        /// </summary>
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
        /// BPM計算後、引数のBPMでの距離Rを返す
        /// </summary>
        /// <param name="bpm"></param>
        /// <returns></returns>
        public double getBpmValue(int bpm)
        {
            if (bpm < _bpmLow || bpm > _bpmHigh)
            {
                return 0;
            }
            return _bpm[bpm - _bpmLow];
        }


        /// <summary>
        /// BPM検出
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public int detect(String fileName)
        {
            _source = CodecFactory.Instance.GetCodec(fileName);
            _fileName = fileName;

            _sampleSource = _source.ToMono().ToSampleSource();
            getVolume();
            getDiff();
            calcBpm();
            findPeaks(_peakWidth);

            double peakMax = 0;
            int bMax = 0;
            foreach (int bpm in _peaks)
            {
                double p = getBpmValue(bpm);

                if (peakMax == 0)
                {
                    peakMax = p;
                    bMax = bpm;
                }
                else if (p / peakMax > _peakThreshold && _priorityBpmLow <= bpm && bpm < _priorityBpmHigh && (bMax % bpm == 0 || bpm % bMax == 0))
                {
                    bMax = bpm;
                }
            }
            return bMax;

        }

        /// <summary>
        /// フレームごとの音量の取得
        /// </summary>
        /// <returns>フレーム数</returns>
        public int getVolume()
        {
            _volume = new List<double>();
            int read;
            float[] buff = new float[_frameSize];
            while ((read = _sampleSource.Read(buff, 0, _frameSize)) == _frameSize)
            {
                double v = 0;
                foreach (float f in buff)
                {
                    v += Math.Abs(f);
                }
                _volume.Add(v);
            }
            return _volume.Count;
        }

        /// <summary>
        /// 音量差分の取得する。同時にハミングウィンドウもかけとく
        /// </summary>
        /// <returns>音量差分リストのサイズ</returns>
        public int getDiff()
        {
            _diff = new List<double>();
            for (int i = 0; i < _volume.Count - 1; i++)
            {
                if (_volume[i + 1] < _volume[i])
                {
                    _diff.Add(0);
                }
                else
                {
                    double ham = 0.54 - 0.46 * Math.Cos(2 * Math.PI * (double)i / ((double)_volume.Count - 1));
                    _diff.Add((_volume[i + 1] - _volume[i]) * ham);
                }
            }
            return _diff.Count;
        }

        /// <summary>
        /// BPM成分抽出
        /// </summary>
        public void calcBpm()
        {
            double s = (double)_sampleSource.WaveFormat.SampleRate / (double)_frameSize;
            _bpm = new List<double>();
            for (int bpm = _bpmLow; bpm <= _bpmHigh; bpm++)
            {
                double a_sum = 0;
                double b_sum = 0;
                double f = (double)bpm / 60;
                for (int n = 0; n < _diff.Count; n++)
                {
                    a_sum += _diff[n] * Math.Cos(2 * Math.PI * f * (double)n / s);
                    b_sum += _diff[n] * Math.Sin(2 * Math.PI * f * (double)n / s);
                }
                double a_tmp = a_sum / (double)_diff.Count;
                double b_tmp = b_sum / (double)_diff.Count;
                _bpm.Add(Math.Sqrt(Math.Pow(a_tmp, 2) + Math.Pow(b_tmp, 2)));
            }
        }

        /// <summary>
        /// BPMピークを求め、ピーク値の大きい順にソートする
        /// </summary>
        /// <param name="width">ピーク判定に使用するデータ数。前後にwidth分を比較し、それらより大きい場合にピークと判定する</param>
        /// <returns>検出したピーク数</returns>
        public int findPeaks(int width)
        {
            _peaks = new List<int>();
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
                        val = _bpm[i - j];
                    }
                    if (val > _bpm[i])
                    {
                        isPeak = false;
                        break;
                    }
                    //after
                    if (i + j < _bpm.Count)
                    {
                        val = _bpm[i + j];
                    }
                    else
                    {
                        val = 0;
                    }
                    if (val > _bpm[i])
                    {
                        isPeak = false;
                        break;
                    }
                }
                if (isPeak)
                {
                    _peaks.Add(i + _bpmLow);
                }
            }
            _peaks.Sort(delegate(int x, int y)
            {
                double d = getBpmValue(y) - getBpmValue(x);// _bpm[y] - _bpm[x];
                if (d == 0) { return 0; }
                else if (d > 0) { return 1; }
                else return -1;
            });

            return _peaks.Count;
        }
    }
}
