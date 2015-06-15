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
using System.Collections.ObjectModel;
using System.IO;

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
            //            PlaylistItem root = new PlaylistItem() { Title = "iTunes", iTunesPlaylist = _itunesApp.LibraryPlaylist };
        }
        void deleteImageDir()
        {
            if (Directory.Exists(_imagePath))
            {
                Directory.Delete(_imagePath, true);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            deleteImageDir();
        }
    }


}
