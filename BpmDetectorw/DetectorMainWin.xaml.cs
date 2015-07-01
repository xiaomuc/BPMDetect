using iTunesLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using SoundAnalyzeLib;

namespace BpmDetectorw
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DetectorMainWin : Window
    {
        iTunesApp _itunesApp;
        string _imagePath;
        Dictionary<int, IBpmDetector> _detectorDictionary;
        BackgroundWorker _backgroundWorker;

        public DetectorMainWin()
        {
            InitializeComponent();
            _itunesApp = new iTunesApp();
            _detectorDictionary = new Dictionary<int, IBpmDetector>();
            PlaylistTreeItem.createPlaylistTree(trvPlayList, _itunesApp.LibrarySource, _detectorDictionary);

            _imagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Properties.Resources.tempImageFolderName);
            deleteImageDir();
            Directory.CreateDirectory(_imagePath);

            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
        }

        void deleteImageDir()
        {
            if (Directory.Exists(_imagePath))
            {
                try
                {
                    Directory.Delete(_imagePath, true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.Write(ex.StackTrace);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            deleteImageDir();
        }

        BPMDetectorConfig createConfig()
        {
            return new BPMDetectorConfig()
            {
                BPMLow = (int)iupBPMLo.Value,
                BPMHigh = (int)iupBPMHi.Value,
                PriorityBPMLow = (int)iupPrioLo.Value,
                PriorityBPMHigh = (int)iupPrioHi.Value,
                PeakThreshold = (double)dupThreshold.Value,
                FrameSize = (int)iupFrameSize.Value,
                AutoCorrelationSize = (int)iupCorrelationSize.Value
            };
        }

        private void lvTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item != null)
            {
                TrackWrapper tw = item.Content as TrackWrapper;
                IITFileOrCDTrack track = tw.Track as IITFileOrCDTrack;
                if (track != null)
                {
                    BPMDetectorConfig config = createConfig();

                    try
                    {
                        //IBpmDetector detector = new BpmDetector(config);
                        IBpmDetector detector = new BPMVolumeAutoCorrelation(config);
                        tw.DetectedBPM = detector.detect(track.Location);
                        tw.Detector = detector;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrames(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
        bool bDetecting = false;
        private void btnDetectAll_Click(object sender, RoutedEventArgs e)
        {
            PlaylistTreeItem item = trvPlayList.SelectedItem as PlaylistTreeItem;
            BackgroundArgument ba = new BackgroundArgument()
            {
                Config = createConfig(),
                Tracks = item.Tracks
            };
            _backgroundWorker.RunWorkerAsync(ba);
            /*
            bDetecting = !bDetecting;
            if (bDetecting)
            {
                BPMDetectorConfig config = createConfig();

                foreach (TrackWrapper tw in lvTracks.Items)
                {
                    if (!bDetecting) break;

                    IITFileOrCDTrack track = tw.Track as IITFileOrCDTrack;
                    if (track != null)
                    {
                        try
                        {
                            // BpmDetector detector = new BpmDetector(config);
                            IBpmDetector detector = new BPMVolumeAutoCorrelation(config);
                            tw.DetectedBPM = detector.detect(track.Location);
                            tw.Detector = detector;
                            lvTracks.SelectedItem = tw;
                            lvTracks.ScrollIntoView(tw);
                            DoEvents();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

            }
            bDetecting = false;
            */
        }
        long prev_tick = 0;
        private void btnTap_Click(object sender, RoutedEventArgs e)
        {
            long now = DateTime.Now.Ticks;
            long tapped = now - prev_tick;
            prev_tick = now;
            if (tapped < TimeSpan.TicksPerMinute)
            {
                long bpm = TimeSpan.TicksPerMinute / tapped;
                tblTap.Text = bpm.ToString();
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
            if (tw != null)
            {
                tw.Track.Play();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _itunesApp.Stop();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int bpm = (int)(sender as Button).Content;
            TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
            if (tw != null)
            {
                tw.DetectedBPM = bpm;
            }
        }

        private void btWriteToiTunes_Click(object sender, RoutedEventArgs e)
        {
            foreach (TrackWrapper tw in lvTracks.Items)
            {
                try
                {
                    if (tw.DetectedBPM != 0)
                    {
                        tw.Track.BPM = tw.DetectedBPM;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
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
            BackgroundWorker worker = sender as BackgroundWorker;
            BackgroundArgument ba = e.Argument as BackgroundArgument;
            foreach (IITFileOrCDTrack track in ba.Tracks)
            {
                BackgroundUserState userState = new BackgroundUserState();
                userState.TrackID = track.trackID;
                userState.Detector = new BPMVolumeAutoCorrelation(ba.Config);
                userState.DetectedBPM = userState.Detector.detect(track.Location);
                worker.ReportProgress(track.Index, userState);
            }
        }
    }
    class BackgroundArgument
    {
        public BPMDetectorConfig Config { get; set; }
        public TrackCollectionWrapper Tracks { get; set; }
    }
    class BackgroundUserState
    {
        public int DetectedBPM { get; set; }
        public int TrackID { get; set; }
        public IBpmDetector Detector { get; set; }
    }
}

