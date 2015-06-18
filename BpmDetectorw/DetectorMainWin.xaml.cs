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

        public DetectorMainWin()
        {
            InitializeComponent();
            _itunesApp = new iTunesApp();
            PlaylistTreeItem.createPlaylistTree(trvPlayList, _itunesApp.LibrarySource);

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

        private void lvTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item != null)
            {
                IITFileOrCDTrack track = item.Content as IITFileOrCDTrack;
                if (track != null)
                {
                    BPMDetectorConfig config = new BPMDetectorConfig()
                    {
                        BPMLow = 80,
                        BPMHigh = 200
                    };
                    BpmDetector detector = new BpmDetector(config);
                    detector.detect(track.Location);
                    TrackToBPMConverter.dictionary.Add(track.trackID, detector);

                    BindingExpression be = bpmSeries1.GetBindingExpression(System.Windows.Controls.DataVisualization.Charting.Series.DataContextProperty);
                    be.UpdateSource();
                    
                    //                    showBPMChart(detector);
                }
            }
        }
        void showBPMChart(BpmDetector detector)
        {
            //            chartBPM.DataContext = detector.BPM;
            bpmSeries1.DataContext = detector.BPM;
            peakSeries.DataContext = detector.TopPeaks;
            /* 
             * Series seriesBPM = _chartBPM.Series["seriesBPM"];
                        seriesBPM.Points.Clear();
                        Series seriesPeak=_chartBPM.Series["seriesPeak"];
                        seriesPeak.Points.Clear();
                        if (detector != null)
                        {
                            ChartArea charAreaBPM = _chartBPM.ChartAreas[0];
                            charAreaBPM.AxisX.Minimum = detector.BpmLow;
                            charAreaBPM.AxisX.Maximum = detector.BpmHigh;
                            for (int bpm = detector.BpmLow; bpm <= detector.BpmHigh; bpm++)
                            {
                                seriesBPM.Points.AddXY(bpm, detector.getBpmValue(bpm));
                            }
                            foreach (int peak in detector.Peaks)
                            {
                                seriesPeak.Points.AddXY(peak, detector.getBpmValue(peak));
                            }
                        }
             */
        }
    }


}
