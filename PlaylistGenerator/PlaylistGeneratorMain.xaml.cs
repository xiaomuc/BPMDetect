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

namespace PlaylistGenerator
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistGeneratorMain : Window
    {
        iTunesApp _app;
        List<GenCode> codeList;
        List<string> _playlistNames;
        public PlaylistGeneratorMain()
        {
            InitializeComponent();
            _app = new iTunesApp();
            codeList = new List<GenCode>();
            _playlistNames=new List<string>();
            foreach(IITPlaylist pl in _app.LibrarySource.Playlists){
                _playlistNames.Add(pl.Name);
            }
            for (int i = 0; i < 3; i++)
            {
                codeList.Add(new GenCode(_playlistNames)
                {
                    Playlist = _playlistNames[i],
                    Duration = 5
                });
            }
            lvCode.ItemsSource = codeList;
            combo.ItemsSource = _playlistNames;
         
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            codeList.Add(new GenCode(_playlistNames));
            lvCode.Items.Refresh();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            GenCode c = lvCode.SelectedItem as GenCode;
            if (c != null)
            {
                codeList.Remove(c);
                lvCode.Items.Refresh();
            }
        }
    }
    class GenCode
    {
        List<string> _playlists;
        public GenCode(List<string> playlists)
        {
            _playlists = playlists;
        }
        public string Playlist { get; set; }
        public int Duration { get; set; }
        public List<string> Playlists { get { return _playlists; } }
    }
}
