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
        iTunesApp _ituneApp;
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
            wt.DetectedBPM = 100;

            IITFileOrCDTrack track = wt.Track as IITFileOrCDTrack;
            CsvWriter writer = CsvWriter.getInstance();
            writer.WaveSourceToCsv(track.Location, @"C:\Users\9500268\Desktop\Music\test.csv");
        }

        private void btnShowWave_Click(object sender, RoutedEventArgs e)
        {
            WrapTrack wt = lvTracks.SelectedItem as WrapTrack;
            IITFileOrCDTrack track = wt.Track as IITFileOrCDTrack;
            double fromSec = (double)iudFromSec.Value;
            double toSec = (double)iudToSec.Value;
            IWaveSource waveSource = CodecFactory.Instance.GetCodec(track.Location);
            txWaveInfo.Text = string.Format("Rate:{0}\nChannels:{1}\nByte/Block:{2}\nByte/Sample:{3}",
                waveSource.WaveFormat.SampleRate,
                waveSource.WaveFormat.Channels,
                waveSource.WaveFormat.BytesPerBlock,
                waveSource.WaveFormat.BytesPerSample);
            ISampleSource sampleSource = waveSource.ToSampleSource();
            int sizeToRead = (int)(sampleSource.WaveFormat.SampleRate * (toSec - fromSec) * sampleSource.WaveFormat.Channels);
            sampleSource.Position = (int)(sampleSource.WaveFormat.SampleRate * fromSec * sampleSource.WaveFormat.Channels);

            int read = 0;
            float[] buffer = new float[sampleSource.WaveFormat.Channels];
            List<KeyValuePair<float, float>> left = new List<KeyValuePair<float, float>>();
            List<KeyValuePair<float, float>> right = new List<KeyValuePair<float, float>>();
            while ((read += sampleSource.Read(buffer, 0, sampleSource.WaveFormat.Channels)) < sizeToRead)
            {
                float pos = (float)sampleSource.Position / (float)(2 * sampleSource.WaveFormat.SampleRate);
                left.Add(new KeyValuePair<float, float>(pos, buffer[0]));
                if (sampleSource.WaveFormat.Channels > 1)
                {
                    right.Add(new KeyValuePair<float, float>(pos, buffer[1]));
                }
            }
            
            seriesLeft.ItemsSource = left;
            seriesRight.ItemsSource = right;
        }
    }
}
