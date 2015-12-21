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
using System.Windows.Shapes;
using iTunesLib;

namespace TagSetter
{
    /// <summary>
    /// AskDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class AskDialog : Window
    {
        public AskDialog()
        {
            InitializeComponent();
        }
        SettingItem item = new SettingItem();
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            item.VoMan = (bool)chbMan.IsChecked;
            item.VoWoman = (bool)chbWoman.IsChecked;
            item.Name = txbArtist.Text;
            item.Lang = txbLang.Text;
            DialogResult = true;
        }
        public void setTrack(IITTrack track)
        {
            txbTitle.Text = track.Name;
            txbAlbum.Text = track.Album;
            txbArtist.Text = track.Artist;
        }
    }
}
