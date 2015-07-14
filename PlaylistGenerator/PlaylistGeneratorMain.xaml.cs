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
using System.IO;
using System.Collections;
using System.Xml.Serialization;
using iTunesLib;

namespace PlaylistGenerator
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistGeneratorMain : Window
    {
        bool _modified = false;
        iTunesApp _app;
        GenerateList _generateList;
        List<string> _playlistNames;
        List<IITTrack> _tracklist;

        public PlaylistGeneratorMain()
        {
            InitializeComponent();
            _app = new iTunesApp();
            _generateList = new GenerateList();
            _playlistNames = new List<string>();
            foreach (IITPlaylist pl in _app.LibrarySource.Playlists)
            {
                _playlistNames.Add(pl.Name);
            }
            lvCode.ItemsSource = _generateList.Items;
            cmbPlaylist.ItemsSource = _playlistNames;

            if (File.Exists(Properties.Settings.Default.CodeFileName))
            {
                loadGeneraterList(Properties.Settings.Default.CodeFileName);
            }
        }
        void dispSourceTime()
        {
            sbiSourceDuration.Content = _generateList.Items.Sum(x => x.Duration).ToString();
        }
        void loadGeneraterList(string fileName)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GenerateList));
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                _generateList = (GenerateList)serializer.Deserialize(fs);
                fs.Close();
            }
            lvCode.ItemsSource = _generateList.Items;
            lvCode.Items.Refresh();
            Properties.Settings.Default.CodeFileName = fileName;
            _modified = false;
            dispSourceTime();
        }

        void saveGeneratorlist()
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.FileName = Properties.Settings.Default.CodeFileName;
            bool? result = saveDialog.ShowDialog();
            if (result == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenerateList));
                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    serializer.Serialize(fs, _generateList);
                    fs.Close();
                }
                Properties.Settings.Default.CodeFileName = saveDialog.FileName;
                _modified = false;
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.FileName = Properties.Settings.Default.CodeFileName;
            bool? result = openDialog.ShowDialog();
            if (result == true)
            {
                loadGeneraterList(openDialog.FileName);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            saveGeneratorlist();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            GenCode g = new GenCode()
            {
                Playlist = (string)cmbPlaylist.SelectedValue,
                Duration = (int)iudDuration.Value
            };
            _generateList.Items.Add(g);
            _modified = true;
            lvCode.Items.Refresh();
            dispSourceTime();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            GenCode c = lvCode.SelectedItem as GenCode;
            if (c != null)
            {
                _generateList.Items.Remove(c);
                lvCode.Items.Refresh();
                _modified = true;
                dispSourceTime();
            }
        }

        private void lvCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCode.SelectedItem != null)
            {
                GenCode g = (GenCode)lvCode.SelectedItem;
                cmbPlaylist.SelectedValue = g.Playlist;
                iudDuration.Value = g.Duration;
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lvCode.SelectedItem != null)
            {
                GenCode g = (GenCode)lvCode.SelectedItem;
                g.Playlist = (string)cmbPlaylist.SelectedValue;
                g.Duration = (int)iudDuration.Value;
                lvCode.Items.Refresh();
                _modified = true;
                dispSourceTime();
            }
        }

        private void btNew_Click(object sender, RoutedEventArgs e)
        {
            _generateList.Items.Clear();
            lvCode.Items.Refresh();
            _modified = true;
            dispSourceTime();
        }
        int _durationWidth = 60;
        int MAX_RETRY = 10;

        private void btnCreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            _tracklist = new List<IITTrack>();
            Random ran = new Random();
            foreach (GenCode g in _generateList.Items)
            {
                IITPlaylist playlist = _app.LibraryPlaylist;
                foreach (IITPlaylist p in _app.LibrarySource.Playlists)
                {
                    if (p.Name.Equals(g.Playlist))
                    {
                        playlist = p;
                        break;
                    }
                }
                List<IITTrack> list = new List<IITTrack>();
                int maxDurationSec = g.Duration * 60 + _durationWidth;
                int minDurationSec = g.Duration * 60 - _durationWidth;
                int du = 0;
                int counter = 0;
                while (du < minDurationSec)
                {
                    int index = ran.Next(playlist.Tracks.Count) + 1;
                    IITTrack track = playlist.Tracks[index];
                    if (!list.Any(x => x.TrackDatabaseID == track.TrackDatabaseID)
                        && !_tracklist.Any(x => x.TrackDatabaseID == track.TrackDatabaseID))
                    {
                        list.Add(track);
                    }
                    du = list.Sum(x => x.Duration);
                    if (du > maxDurationSec)
                    {
                        list.Clear();
                        du = 0;
                        counter++;
                        if (counter > MAX_RETRY)
                        {
                            maxDurationSec += _durationWidth / 2;
                            minDurationSec -= _durationWidth / 2;
                            counter = 0;
                        }
                    }
                }
                _tracklist.AddRange(list);
            }
            TimeSpan ts = new TimeSpan(0, 0, _tracklist.Sum(x => x.Duration));
            sbiPlaylistDuration.Content = ts.ToString();
            lvGeneratedPlaylist.ItemsSource = _tracklist;
        }

        private void btnWriteToitunes_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_modified)
            {
                saveGeneratorlist();
            }
            Properties.Settings.Default.Save();
        }
        char[] SPLIT_CHAR = { '\\', '/' };
        void getSavePlaylist(string value, out string parent, out string target)
        {
            parent = string.Empty;
            target = string.Empty;
            if (value.Contains('\\') || value.Contains('/'))
            {
                string[] s = value.Split(SPLIT_CHAR);
                parent = s[0];
                target = s[1];
            }
        }

        private void btnWriteOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string parent, target;
                getSavePlaylist(txbPlaylistName.Text, out parent, out target);
                IITPlaylist plParent = _app.LibrarySource.Playlists.get_ItemByName(parent);
                IITUserPlaylist plTarget = _app.LibrarySource.Playlists.get_ItemByName(target) as IITUserPlaylist;
                if (plTarget != null && plTarget.get_Parent() != null && plTarget.get_Parent().Equals(plParent))
                {
                    plTarget.Delete();
                }
                plTarget = _app.CreatePlaylist(target) as IITUserPlaylist;
                plTarget.set_Parent(plParent);
                foreach (IITTrack track in _tracklist)
                {
                    plTarget.AddTrack(track);
                }
            }
            finally { InputBox.Visibility = System.Windows.Visibility.Collapsed; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Collapsed;

        }
    }

    public class GenerateList
    {
        public List<GenCode> Items = new List<GenCode>();

    }
    public class GenCode
    {
        public string Playlist { get; set; }
        public int Duration { get; set; }
    }
}
