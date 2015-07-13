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
        List<GenCode> codeList;
        List<string> _playlistNames;
        public PlaylistGeneratorMain()
        {
            InitializeComponent();
            _app = new iTunesApp();
            codeList = new List<GenCode>();
            _playlistNames = new List<string>();
            foreach (IITPlaylist pl in _app.LibrarySource.Playlists)
            {
                _playlistNames.Add(pl.Name);
            }
            for (int i = 0; i < 3; i++)
            {
                codeList.Add(new GenCode()
                {
                    Playlist = _playlistNames[i],
                    Duration = 5,
                });
            }
            lvCode.ItemsSource = codeList;
            cmbPlaylist.ItemsSource = _playlistNames;

        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            bool? result = saveDialog.ShowDialog();
            if (result == true)
            {
                using (Stream stream = new FileStream(saveDialog.SafeFileName, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        Type[] et = new Type[] { typeof(GenCode) };
                        XmlSerializer serializer = new XmlSerializer(typeof(List<GenCode>), et);
                        serializer.Serialize(writer, codeList);
                        writer.Close();
                    }
                    stream.Close();
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
            codeList.Add(g);
            lvCode.Items.Refresh();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            GenCode c = lvCode.SelectedItem as GenCode;
            if (c != null)
            {
                codeList.Remove(c);
                lvCode.Items.Refresh();
            }
        }
    }
    public class GenCode
    {
        public string Playlist { get; set; }
        public int Duration { get; set; }
    }
}
