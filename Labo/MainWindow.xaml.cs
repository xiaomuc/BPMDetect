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
            showVolume();
        }
        void showWave()
        {
            int frameSize = (int)iudFrameSize.Value;
            int frameCount = (int)iudFrameCount.Value * 2;
            float[] buffer = new float[frameSize * frameCount];
            float fromSec = (float)_sampleSource.GetPosition().TotalSeconds;
            int read;
            if ((read = _sampleSource.Read(buffer, 0, frameSize * frameCount)) > 0)
            {
                Dictionary<float, float> left = buffer.Where((value, index) => index % 2 == 0)
                    .Select((x, i) => new { x, i }).ToDictionary(a => fromSec + (float)a.i / (float)_sampleSource.WaveFormat.SampleRate, a => a.x);
                seriesVolume.ItemsSource = left;
                Dictionary<float, float> right = buffer.Where((value, index) => index % 2 == 1)
                    .Select((x, i) => new { x, i }).ToDictionary(a => fromSec + (float)a.i / (float)_sampleSource.WaveFormat.SampleRate, a => a.x);
                seriesVolAc.ItemsSource = right;
            }
        }
        void showVolume()
        {
            int frameSize = (int)iudFrameSize.Value;
            int frameCount = (int)iudFrameCount.Value * 2;
            float[] buffer = new float[frameSize];
            Dictionary<double, double> vol = new Dictionary<double, double>();
            Dictionary<double, double> diff = new Dictionary<double, double>();
            double s = (double)_sampleSource.WaveFormat.SampleRate / (double)frameSize;
            double volPrev = 0;
            for (int f = 0; f < frameCount; f++)
            {
                int read;
                double tm = (double)f / s;
                if ((read = _sampleSource.Read(buffer, 0, frameSize)) > 0)
                {
                    float[] left = buffer.Where((v, i) => i % 2 == 0).ToArray();
                    float[] right = buffer.Where((v, i) => i % 2 == 1).ToArray();

                    double normL = Math.Sqrt(left.Zip(left, (i, j) => i * j).Sum() / (double)frameSize);
                    double normR = Math.Sqrt(right.Zip(right, (i, j) => i * j).Sum() / (double)frameSize);
                    double volNow = normL + normR;
                    vol.Add(tm, volNow);
                    if (volNow > volPrev)
                    {
                        diff.Add(tm, volNow - volPrev);
                    }
                    else
                    {
                        diff.Add(tm, 0);
                    }
                    volPrev = volNow;
                }
            }
            seriesVolume.ItemsSource = vol;
            Dictionary<double, double> volAc = new Dictionary<double, double>();
            double norm = vol.Zip(vol, (i, j) => i.Value * j.Value).Sum();
            for (int n = 1; n < 1000; n++)
            {
                double inp = vol.Zip(vol.Skip(n), (i, j) => i.Value * j.Value).Sum();
                double tm = (double)n * (double)frameSize / (double)_sampleSource.WaveFormat.SampleRate;
                volAc.Add(tm, inp / norm);
            }
            Dictionary<double, double> volAcLinear = calcAB(volAc);
            seriesVolAc.ItemsSource = volAc;
            //            BpmDetector detector = new BpmDetector(new BPMDetectorConfig());
            //            Dictionary<int, double> bpmList = detector.calculateBPM(diff, _sampleSource.WaveFormat.SampleRate, frameSize);
            seriesVolLinear.ItemsSource = volAcLinear;
            Dictionary<double, double> volAcNorm = volAc.Zip(volAcLinear, (i, j) => new { Key = i.Key, Value = i.Value - j.Value }).ToDictionary(x => x.Key, x => x.Value);
            seriesVolAcNorm.ItemsSource = volAcNorm;

            //            Dictionary<double, double> volHam = volAc.Zip(getHamming(volAc.Count), (i, j) => new { Key = i.Key, Value = i.Value * j }).ToDictionary(x => x.Key, x => x.Value);
            Dictionary<double, double> volHam = volAc.Select((x, i) => new { x, i }).ToDictionary(y => y.x.Key, y => getHamming(y.x.Value, y.i, volAc.Count));
            seriesVolAcHam.ItemsSource = volHam;
        }
        double getHamming(double value, int index, int count)
        {
            double h = getHamming(index, count);
            double ret = value * h;
            return ret;
        }
        double getHamming(int index, int count)
        {
            return 0.54 - 0.46 * Math.Cos(2 * Math.PI * (double)index / (double)(count - 1));
        }
        List<double> getHamming(int count)
        {
            List<double> list = new List<double>();
            for (int i = 0; i < count; i++)
            {
                list.Add(0.54 - 0.46 * Math.Cos(2 * Math.PI * (double)i / (double)(count - 1)));
            }
            return list;
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
