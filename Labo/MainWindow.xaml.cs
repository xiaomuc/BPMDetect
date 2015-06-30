using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using iTunesLib;
using SoundAnalyzeLib;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams;

namespace Labo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        iTunesApp _ituneApp;
        ISampleSource _sampleSource;
        BackgroundWorker _backgroundWorker;
        
        public MainWindow()
        {
            InitializeComponent();
            _ituneApp = new iTunesApp();
            IITTrackCollection trackCollection = _ituneApp.LibraryPlaylist.Tracks;

            IITPlaylist p = _ituneApp.LibrarySource.Playlists.get_ItemByName("次郎");
            if (p != null)
            {
                trackCollection = p.Tracks;
            }

            WrapCollection collection = new WrapCollection(trackCollection);

            lvTracks.ItemsSource = collection;
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
        }

        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void lvTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WrapTrack wt = lvTracks.SelectedItem as WrapTrack;

            IITFileOrCDTrack track = wt.Track as IITFileOrCDTrack;
            try
            {
                IWaveSource waveSource = CodecFactory.Instance.GetCodec(track.Location);
                _sampleSource = waveSource.ToSampleSource();

                showVolume(track.BPM);
                wt.DetectedBPM= showVolByDetector(track.Location);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void showVolume(int b)
        {
            int frameSize = (int)iudFrameSize.Value;
            int autoCoSize = (int)iudFrameCount.Value;
            int bpmLo = (int)iudBpmLo.Value;
            int bpmHi = (int)iudBpmHi.Value;
            Console.WriteLine("ShowVolume:FrameSize={0},AutoCorretationSize={1}", frameSize, autoCoSize);
            float[] buffer = new float[frameSize*2];
            Dictionary<double, double> volume = new Dictionary<double, double>();
            //Dictionary<double, double> diff = new Dictionary<double, double>();
            double s = (double)_sampleSource.WaveFormat.SampleRate / (double)frameSize;
            double volPrev = 0;
            int read;
            int frameCounter = 0;
            Console.WriteLine("Read start:");
            //音量差分の検出
            double tm = _sampleSource.GetPosition().TotalSeconds;
            while ((read = _sampleSource.Read(buffer, 0, frameSize * 2)) == frameSize * 2)
            {
                //                double tm = (double)frameCounter / s;
                float[] left = buffer.Where((v, i) => i % 2 == 0).ToArray();
                float[] right = buffer.Where((v, i) => i % 2 == 1).ToArray();

                double normL = Math.Sqrt(left.Zip(left, (i, j) => i * j).Sum() / (double)frameSize);
                double normR = Math.Sqrt(right.Zip(right, (i, j) => i * j).Sum() / (double)frameSize);
                double volNow = normL + normR;
                //volume.Add(tm, volNow);
                if (volNow > volPrev)
                {
                    volume.Add(tm, volNow - volPrev);
                }
                else
                {
                    volume.Add(tm, 0);
                }
                volPrev = volNow;
                frameCounter++;
                tm = _sampleSource.GetPosition().TotalSeconds;
            }
            Console.WriteLine("show volume chart.");
            //seriesVolume.ItemsSource = vol;

            //音量差分の自己相関
            Console.WriteLine("calc volume auto-correlation.");
            Dictionary<double, double> volAutoCorrelation = calcAutoCorrelation(volume, autoCoSize, _sampleSource.WaveFormat.SampleRate, frameSize);
            //一次近似
            Console.WriteLine("calc volume linear.");
            Dictionary<double, double> volAutoCorrelationLinear = calcAB(volAutoCorrelation);
            Console.WriteLine("show volume auto-correlation and linear chart.");
            seriesDiffAutoCorrelation.ItemsSource = volAutoCorrelation;
            seriesDiffLinear.ItemsSource = volAutoCorrelationLinear;
            //自己相関を一次近似直線により正規化
            Console.WriteLine("calc volume auto-correlation normalized.");
            Dictionary<double, double> volAutoCorrelationNorm
                = volAutoCorrelation.Zip(volAutoCorrelationLinear, (i, j) => new { Key = i.Key, Value = i.Value - j.Value })
                //.Select((x, i) => new { x.Key, Value = x.Value * (0.54 - 0.46 * Math.Cos(2 * Math.PI * (double)i / (double)volAutoCorrelation.Count)) })
                .ToDictionary(x => x.Key, x => x.Value);
            Console.WriteLine("show volume auto-correlation normalized chart.");
            seriesDiffAcNorm.ItemsSource = volAutoCorrelationNorm;

            //差分を使ったBPM算出
            Console.WriteLine("calc bpm by no hamming.");
            Dictionary<int, double> volBPM = calculateBPM(volAutoCorrelationNorm, _sampleSource.WaveFormat.SampleRate, frameSize, bpmLo, bpmHi);
            Console.WriteLine("show BPM-No-hamming chart.");
            seriesDiffBpm.ItemsSource = volBPM;

            #region Diffs
            /*
            //音量差分を使った計算
            //Console.WriteLine("show diff chart");
            //seriesDiff.ItemsSource = diff;
            //音量差分での自己相関
            Console.WriteLine("calc diff auto-correlation");
            Dictionary<double, double> diffAutoCorrelation = calcAutoCorrelation(diff, autoCoSize, _sampleSource.WaveFormat.SampleRate, frameSize);
            Console.WriteLine("show diff auto-correlation chart");
            seriesDiffAutoCorrelation.ItemsSource = diffAutoCorrelation;

            //差分の一次近似
            Console.WriteLine("calc Diff linear.");
            Dictionary<double, double> diffAutoCorrelationLinear = calcAB(diffAutoCorrelation);
            Console.WriteLine("show diff linear chart.");
            seriesDiffLinear.ItemsSource = diffAutoCorrelationLinear;
            //音量自己相関を一次近似直線により正規化
            Console.WriteLine("calc volume auto-correlation normalized.");
            Dictionary<double, double> diffAutoCorrelationNorm
                = diffAutoCorrelation.Zip(diffAutoCorrelationLinear, (i, j) => new { Key = i.Key, Value = i.Value - j.Value })
                //.Select((x, i) => new { x.Key, Value = x.Value * (0.54 - 0.46 * Math.Cos(2 * Math.PI * (double)i / (double)diffAutoCorrelation.Count)) })
                .ToDictionary(x => x.Key, x => x.Value);
            Console.WriteLine("show volume auto-correlation normalized chart.");
            seriesDiffAcNorm.ItemsSource = diffAutoCorrelationNorm;


            Console.WriteLine("calc diff bpm");
            Dictionary<int, double> diffBpm = calculateBPM(diffAutoCorrelationNorm, _sampleSource.WaveFormat.SampleRate, frameSize, bpmLo, bpmHi);
            Console.WriteLine("show diff bpm");
            seriesDiffBpm.ItemsSource = diffBpm;
            */
            #endregion

            Console.WriteLine("Volume BPM Peaks");
            Dictionary<int, double> volBpmPeaks = getPeaks(volBPM);
            diffPeakList.ItemsSource = volBpmPeaks;
            //Dictionary<int, double> sortedVolBpm = bpmVolACNoHarmBPM.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            Console.WriteLine("Volume BPM Peaks");
            foreach (KeyValuePair<int, double> x in volBpmPeaks)
            {
                if (x.Value > 0)
                {
                    Console.WriteLine("[{0}]:{1}", x.Key, x.Value);
                }
            }

            /*
            Console.WriteLine("Diff BPM Peaks");
            Dictionary<int, double> diffBpmPeaks = getPeaks(diffBpm);
            diffPeakList.ItemsSource = diffBpmPeaks;
            //Dictionary<int, double> sortedDiffBpm = diffBpm.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            Console.WriteLine("Diff BPM Peaks");
            foreach (KeyValuePair<int, double> x in diffBpmPeaks)
            {
                if (x.Value > 0)
                {
                    Console.WriteLine("[{0}]:{1}", x.Key, x.Value);
                }
            }
            */
        }
        int showVolByDetector(string fileName)
        {
            int frameSize = (int)iudFrameSize.Value;
            int autoCoSize = (int)iudFrameCount.Value;
            int bpmLo = (int)iudBpmLo.Value;
            int bpmHi = (int)iudBpmHi.Value;
            BPMVolumeAutoCorrelation detector =
                new BPMVolumeAutoCorrelation(
                    new BPMDetectorConfig()
                        {
                            FrameSize = frameSize,
                            BPMLow = bpmLo,
                            BPMHigh = bpmHi,
                            AutoCorrelationSize = autoCoSize
                        });
            int bpm=detector.detect(fileName);
            seriesVolAc.ItemsSource = detector.AutoCorrelation;
            seriesVolAcNorm.ItemsSource = detector.Normalized;
            seriesVolAcBpm.ItemsSource = detector.BPM;
            volumePeakList.ItemsSource = detector.TopPeaks;
            return bpm;
        }
        Dictionary<int, double> getPeaks(Dictionary<int, double> data)
        {

            return data.Skip(1).Take(data.Count - 2).Where(x => data[x.Key - 1] < x.Value && data[x.Key + 1] < x.Value).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        Dictionary<double, double> calcAutoCorrelation(Dictionary<double, double> data, int autoCoSize, int sampleRate, int frameSize)
        {
            Dictionary<double, double> autoCorrelation = new Dictionary<double, double>();
            double norm = data.Zip(data, (i, j) => i.Value * j.Value).Sum() / (double)data.Count;
            double ts = (double)frameSize / (double)sampleRate;
            for (int n = 0; n < autoCoSize; n++)
            {
                double inp = data.Zip(data.Skip(n), (i, j) => i.Value * j.Value).Sum() / (double)(data.Count - n);
                //                double tm = (double)n * ts;
                double tm = data.ElementAt(n).Key;
                autoCorrelation.Add(tm, inp / norm);
            }
            return autoCorrelation;
        }

        public Dictionary<int, double> calculateBPM(Dictionary<double, double> diff, int sampleRate, int frameSize, int bpmLow = 60, int bpmHigh = 200)
        {
            double s = (double)sampleRate / (double)frameSize;
            Dictionary<int, double> bpmList = new Dictionary<int, double>();
            for (int bpm = bpmLow; bpm <= bpmHigh; bpm++)
            {
                double f = (double)bpm / 60;
                double a_sum =
                    //diff.Select((x, i) => x.Value * Math.Cos(2.0 * Math.PI * f * (double)i / s)).Sum();
                    diff.Select((x, i) => x.Value * Math.Cos(2.0 * Math.PI * f * x.Key)).Sum();
                Console.WriteLine("{0}BPM:{1}", bpm, a_sum);
                bpmList.Add(bpm, a_sum);
            }
            double m = bpmList.Max(x => x.Value);
            return bpmList.ToDictionary(x => x.Key, x => x.Value / m);
        }

        Dictionary<double, double> calcAB(Dictionary<double, double> data)
        {
            double n = (double)data.Count;
            //
            double P = data.Sum(d => d.Key);
            double Q = data.Sum(d => d.Value);
            double R = data.Sum(d => d.Key * d.Value);
            double S = data.Sum(d => d.Key * d.Key);

            double a = (n * R - P * Q) / (n * S - P * P);
            double b = (S * Q - R * P) / (n * S - P * P);
            return data.ToDictionary(x => x.Key, x => x.Key * a + b);
        }

        private void btnShowWave_Click(object sender, RoutedEventArgs e)
        {
            //            showWave();
        }

        private void iudFromSec_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void iudToSec_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }
    }
}
