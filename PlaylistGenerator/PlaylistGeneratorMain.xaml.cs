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
        iTunesApp _app;

        [System.Xml.Serialization.XmlArrayItem(typeof(GenCode))]
        GenList codeList;
        List<string> _playlistNames;
        public PlaylistGeneratorMain()
        {
            InitializeComponent();
            _app = new iTunesApp();
            codeList = new GenList();
            _playlistNames = new List<string>();
            foreach (IITPlaylist pl in _app.LibrarySource.Playlists)
            {
                _playlistNames.Add(pl.Name);
            }
            for (int i = 0; i < 3; i++)
            {
                codeList.Items.Add(new GenCode()
                {
                    Playlist = _playlistNames[i],
                    Duration = 5,
                });
            }
            lvCode.ItemsSource = codeList.Items;
            cmbPlaylist.ItemsSource = _playlistNames;

        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
            bool? result = openDialog.ShowDialog();
            if (result == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenList));
                using (FileStream fs = new FileStream(openDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    codeList = (GenList)serializer.Deserialize(fs);
                    fs.Close();
                }
                lvCode.ItemsSource = codeList.Items;
                lvCode.Items.Refresh();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            bool? result = saveDialog.ShowDialog();
            if (result == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenList));
                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    serializer.Serialize(fs, codeList);
                    fs.Close();
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            GenCode g = new GenCode()
            {
                Playlist = (string)cmbPlaylist.SelectedValue,
                Duration = (int)iudDuration.Value
            };
            codeList.Items.Add(g);
            lvCode.Items.Refresh();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            GenCode c = lvCode.SelectedItem as GenCode;
            if (c != null)
            {
                codeList.Items.Remove(c);
                lvCode.Items.Refresh();
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

        }
    }

    public class GenList
    {
        public List<GenCode> Items = new List<GenCode>();

    }
    public class GenCode
    {
        public string Playlist { get; set; }
        public int Duration { get; set; }
    }
}
