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
using System.IO;
using Labo.Wrapper;

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
        TrackCollectionWrapper _collection;
        Dictionary<int, IBpmDetector> detectorDictionary;
        string _playImageFile = "1435735759_play.png";
        string _pauseImageFile = "1435735785_pause.png";
        BitmapImage biPlay;
        BitmapImage biPause;
        string _dataPath;

        public MainWindow()
        {
            InitializeComponent();
            _ituneApp = new iTunesApp();
            Dictionary<string, TrackCollectionWrapper> comboList = new Dictionary<string, TrackCollectionWrapper>();
            detectorDictionary = new Dictionary<int, IBpmDetector>();
            foreach (IITPlaylist pl in _ituneApp.LibrarySource.Playlists)
            {
                TrackCollectionWrapper collection = new TrackCollectionWrapper(pl.Tracks, detectorDictionary);
                if (!comboList.ContainsKey(pl.Name))
                {
                    comboList.Add(pl.Name, collection);
                }
            }
            cmbPlaylist.ItemsSource = comboList;

            //lvTracks.ItemsSource = _collection;

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;

            _ituneApp.OnPlayerPlayEvent += _ituneApp_OnPlayerPlayEvent;
            _ituneApp.OnPlayerPlayingTrackChangedEvent += _ituneApp_OnPlayerPlayingTrackChangedEvent;
            _ituneApp.OnPlayerStopEvent += _ituneApp_OnPlayerStopEvent;

            biPlay = new BitmapImage(new Uri(_playImageFile, UriKind.Relative));
            biPause = new BitmapImage(new Uri(_pauseImageFile, UriKind.Relative));
            _dataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "BpmDetecor");
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
            sub.ChildClass c = new sub.ChildClass();
            c.writeConsole();
        }
        string getDataPath(IITTrack track)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(track.Artist + "_" + track.Album + "_" + track.Name + ".dat");
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                sb.Replace(c, '~');
            }
            return System.IO.Path.Combine(_dataPath, sb.ToString());
        }
        private void lvTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TrackWrapper wt = lvTracks.SelectedItem as TrackWrapper;

            IITFileOrCDTrack track = wt.Track as IITFileOrCDTrack;
            try
            {
                IWaveSource waveSource = CodecFactory.Instance.GetCodec(track.Location);
                _sampleSource = waveSource.ToSampleSource();

                //showVolume(track.BPM);

                wt.DetectedBPM = showVolByDetector(track,
                    BPMVolumeAutoCorrelation.wave8beat,
                    seriesVolAc,seriesVolAcNorm,seriesVolAcBpm);
                wt.DetectedBPM = showVolByDetector(track,
                    BPMVolumeAutoCorrelation.wave3note,
                    seriesDiffAutoCorrelation, seriesDiffAcNorm, seriesDiffBpm);
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
            float[] buffer = new float[frameSize * 2];
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
        int showVolByDetector(IITFileOrCDTrack track, BeatWave bw,
            System.Windows.Controls.DataVisualization.Charting.LineSeries seriesAc,
            System.Windows.Controls.DataVisualization.Charting.LineSeries seriesNorm,
            System.Windows.Controls.DataVisualization.Charting.LineSeries seriesBpm)
        {
            DateTime st = DateTime.Now;
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
            Console.WriteLine((DateTime.Now - st).ToString() + " Detect Start");
            int bpm = detector.detect(track.Location);
            string saveFileName = getDataPath(track);
            Console.WriteLine((DateTime.Now - st).ToString() + " Save Start");
            detector.saveToFile(saveFileName);
            Console.WriteLine((DateTime.Now - st).ToString() + " Chart Start");
            seriesAc.ItemsSource = detector.AutoCorrelation;
            seriesNorm.ItemsSource = detector.Normalized;
            seriesBpm.ItemsSource = detector.BPMs;
            volumePeakList.ItemsSource = detector.TopPeaks;
            Console.WriteLine((DateTime.Now - st).ToString() + " end.");

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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.Equals("Start"))
            {
                int frameSize = (int)iudFrameSize.Value;
                int autoCoSize = (int)iudFrameCount.Value;
                int bpmLo = (int)iudBpmLo.Value;
                int bpmHi = (int)iudBpmHi.Value;
                BPMDetectorConfig config = new BPMDetectorConfig()
                                {
                                    FrameSize = frameSize,
                                    BPMLow = bpmLo,
                                    BPMHigh = bpmHi,
                                    AutoCorrelationSize = autoCoSize
                                };
                TrackCollectionWrapper collection = lvTracks.ItemsSource as TrackCollectionWrapper;

                object[] param = new object[2];
                param[0] = collection;
                param[1] = config;
                progressBar.Maximum = collection.Count;
                progressBar.Value = 0;
                btnStart.Content = "Stop";
                _backgroundWorker.RunWorkerAsync(param);
            }
            else
            {
                btnStart.IsEnabled = false;
                _backgroundWorker.CancelAsync();
            }
        }
        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnStart.Content = "Start";
            btnStart.IsEnabled = true;
        }

        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            // IBpmDetector detector = e.UserState as IBpmDetector;
            TrackWrapper tw = e.UserState as TrackWrapper;

            foreach (TrackWrapper item in lvTracks.Items)
            {
                if (item.Track.trackID == tw.Track.trackID)
                {
                    item.DetectedBPM = tw.DetectedBPM;
                }
            }

            //TrackCollectionWrapper collection = lvTracks.ItemsSource as TrackCollectionWrapper;
            //foreach (TrackWrapper item in collection)
            //{
            //    if (item.Track.trackID.Equals(tw.Track.trackID))
            //    {
            //        item.Detector = detector;
            //        item.DetectedBPM = detector.TopPeaks.First().Key;
            //    }
            //}
        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            object[] param = (object[])e.Argument;
            TrackCollectionWrapper collection = param[0] as TrackCollectionWrapper;
            BPMDetectorConfig config = param[1] as BPMDetectorConfig;
            int i = 0;
            foreach (TrackWrapper tw in collection)
            {
                if (worker.CancellationPending) break;
                IITFileOrCDTrack track = tw.Track as IITFileOrCDTrack;
                IBpmDetector detector = new BPMVolumeAutoCorrelation(config);
                tw.DetectedBPM = detector.detect(track.Location);
                detector.ID = tw.Track.trackID;
                worker.ReportProgress(++i, tw);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_ituneApp.PlayerState == ITPlayerState.ITPlayerStateStopped)
            {
                _ituneApp.Play();
            }
            else if (_ituneApp.PlayerState == ITPlayerState.ITPlayerStatePlaying)
            {
                _ituneApp.Pause();
            }
        }
        void _ituneApp_OnPlayerStopEvent(object iTrack)
        {
            imgPlayPause.Dispatcher.BeginInvoke(new Action(() =>
            {
                imgPlayPause.Source = biPlay;
            }));
        }

        void _ituneApp_OnPlayerPlayingTrackChangedEvent(object iTrack)
        {

            Console.WriteLine("track changed:");
        }

        void _ituneApp_OnPlayerPlayEvent(object iTrack)
        {
            imgPlayPause.Dispatcher.BeginInvoke(new Action(() =>
            {
                //BitmapImage bi = new BitmapImage(new Uri("1435735785_pause.png", UriKind.Relative));
                imgPlayPause.Source = biPause;
            }));
            lvTracks.Dispatcher.BeginInvoke(
                     new Action(() =>
                     {
                         IITTrack track = iTrack as IITTrack;
                         foreach (TrackWrapper tw in lvTracks.Items)
                         {
                             if (tw.Track.TrackDatabaseID == track.TrackDatabaseID)
                             {
                                 lvTracks.SelectedItem = tw;
                                 lvTracks.ScrollIntoView(tw);
                                 break;
                             }
                         }
                     }));
        }

        private void lvTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime st = DateTime.Now;
            TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
            string fileName = getDataPath(tw.Track);
            if (File.Exists(fileName))
            {
                BPMVolumeAutoCorrelation detector = new BPMVolumeAutoCorrelation();
                detector.loadFromFile(fileName);
                tw.Detector = detector;
                seriesVolAc.ItemsSource = detector.AutoCorrelation;
                seriesVolAcNorm.ItemsSource = detector.Normalized;
                seriesVolAcBpm.ItemsSource = detector.BPMs;
                volumePeakList.ItemsSource = detector.TopPeaks;
                Console.WriteLine((DateTime.Now - st).ToString() + " File Load end");
            }
        }
    }
}
