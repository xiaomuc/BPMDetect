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
            WrapTrack wt= lvTracks.SelectedItem as WrapTrack;
            wt.DetectedBPM = 100;

            IITFileOrCDTrack track = wt.Track as IITFileOrCDTrack;
            CsvWriter writer = CsvWriter.getInstance();
            writer.WaveSourceToCsv(track.Location, @"C:\Users\9500268\Desktop\Music\test.csv");
        }
    }
}
