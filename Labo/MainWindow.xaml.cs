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
        int MAX_SIZE = 33554432;
        iTunesApp _ituneApp;
        List<float> leftList;
        List<float> rightList;
        ISampleSource _sampleSource;
        public MainWindow()
        {
            InitializeComponent();
            _ituneApp = new iTunesApp();
            WrapCollection collection = new WrapCollection(_ituneApp.LibraryPlaylist.Tracks);

            lvTracks.ItemsSource = collection;
        }

        private void lvTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            WrapTrack wt = lvTracks.SelectedItem as WrapTrack;

            IITFileOrCDTrack track = wt.Track as IITFileOrCDTrack;
            IWaveSource waveSource = CodecFactory.Instance.GetCodec(track.Location);
            _sampleSource = waveSource.ToSampleSource();
            txWaveInfo.Text = string.Format("{0}[Hz] Ch:{1}",
                     waveSource.WaveFormat.SampleRate,
                     waveSource.WaveFormat.Channels);
            iudFrameCount.Value = (int)_sampleSource.Length / (int)iudFrameSize.Value;
            btnShowWave.IsEnabled = true;
        }
        void showWave()
        {
            int frameSize = (int)iudFrameSize.Value;
            int frameCount = (int)iudFrameCount.Value*2;
            float[] buffer = new float[frameSize * frameCount];
            float fromSec = (float)_sampleSource.GetPosition().TotalSeconds;
            int read;
            if ((read = _sampleSource.Read(buffer, 0, frameSize * frameCount)) > 0)
            {
                Dictionary<float, float> left = buffer.Where((value, index) => index % 2 == 0)
                    .Select((x, i) => new { x, i }).ToDictionary(a => fromSec + (float)a.i / (float)_sampleSource.WaveFormat.SampleRate, a => a.x);
                seriesLeft.ItemsSource = left;
                Dictionary<float, float> right = buffer.Where((value, index) => index % 2 == 1)
                    .Select((x, i) => new { x, i }).ToDictionary(a => fromSec + (float)a.i / (float)_sampleSource.WaveFormat.SampleRate, a => a.x);
                seriesRight.ItemsSource = right;
            }
        }
        void showVolume()
        {
            int frameSize = (int)iudFrameSize.Value;
            int frameCount = (int)iudFrameCount.Value * 2;
            float[] buffer = new float[frameSize];
            Dictionary<double, double> dict = new Dictionary<double, double>();
            for (int f = 0; f < frameCount; f++)
            {
                int read;
                double fromSec = _sampleSource.GetPosition().TotalSeconds;
                if ((read = _sampleSource.Read(buffer, 0, frameSize)) > 0)
                {
                    float[] left = buffer.Where((v, i) => i % 2 == 0).ToArray();
                    float[] right = buffer.Where((v, i) => i % 2 == 1).ToArray();

                    double normL = Math.Sqrt(left.Zip(left, (i, j) => i * j).Sum());
                    double normR = Math.Sqrt(right.Zip(right, (i, j) => i * j).Sum());
                    dict.Add(fromSec, normL+normR);
                }
            }
            seriesLeft.ItemsSource = dict;
            Dictionary<double, double> acDict = new Dictionary<double, double>();
            double norm = dict.Zip(dict, (i, j) => i.Value * j.Value).Sum();
            for (int n = 1; n < 1000; n++)
            {
                double inp = dict.Zip(dict.Skip(n), (i, j) => i.Value * j.Value).Sum();
                double tm = (double)n * (double)frameSize / (double)_sampleSource.WaveFormat.SampleRate;
                acDict.Add(tm, inp/norm);
            }
            seriesRight.ItemsSource = acDict;
            BpmDetector detector = new BpmDetector(new BPMDetectorConfig());
            Dictionary<int,double>bpmList= detector.calculateBPM(acDict, _sampleSource.WaveFormat.SampleRate, frameSize);
            seriesBPM.ItemsSource = bpmList;
        }

        private void btnShowWave_Click(object sender, RoutedEventArgs e)
        {
//            showWave();
            showVolume();
        }

        private void iudFromSec_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.IsInitialized)
            {
                showWave();
            }
        }

        private void iudToSec_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }
    }
}
