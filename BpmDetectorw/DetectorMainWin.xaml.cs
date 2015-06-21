using iTunesLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        Dictionary<int, BpmDetector> _detectorDictionary;

        public DetectorMainWin()
        {
            InitializeComponent();
            _itunesApp = new iTunesApp();
            _detectorDictionary = new Dictionary<int, BpmDetector>();
            PlaylistTreeItem.createPlaylistTree(trvPlayList, _itunesApp.LibrarySource, _detectorDictionary);

            _imagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Properties.Resources.tempImageFolderName);
            deleteImageDir();
            Directory.CreateDirectory(_imagePath);

            //_chartBPM = (Chart)HostChart.Child;
            //_chartBPM.ChartAreas[0].AxisX.Interval = 10D;
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
                PeakThreshold = (double)dupThreshold.Value
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
                        BpmDetector detector = new BpmDetector(config);
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
                            BpmDetector detector = new BpmDetector(config);
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
    }
}

