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
using iTunesLib;

namespace Labo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        iTunesApp _app;
        public MainWindow()
        {
            InitializeComponent();
            _app = new iTunesApp();

            txbConsole.AppendText("Sources" + Environment.NewLine);
            foreach (IITSource source in _app.Sources)
            {
                
                txbConsole.AppendText(source.Name + Environment.NewLine);
            }
            IITTrackCollection tc = _app.LibraryPlaylist.Search("a", ITPlaylistSearchField.ITPlaylistSearchFieldArtists);
            Dictionary<int, int> tempList = new Dictionary<int, int>();
            foreach (IITTrack track in tc)
            {
                txbConsole.AppendText(track.Name + "/" + track.Artist + Environment.NewLine);
            }
            lbxTracks.ItemsSource = _app.LibraryPlaylist.Tracks;
            lvTracks.ItemsSource = _app.LibraryPlaylist.Tracks;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lbxTracks.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Album");
            view.GroupDescriptions.Add(groupDescription);
           
            int t = 0;
            foreach (IITTrack track in _app.LibraryPlaylist.Tracks)
            {
                tempList.Add(t += track.Duration, track.BPM);
            }

            lineSeries.ItemsSource = tempList;
        
        }
    }
}
