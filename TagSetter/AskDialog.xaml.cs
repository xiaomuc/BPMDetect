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
        public AskDialog(IITTrack track)
        {
            InitializeComponent();
            setTrack(track);
        }
        SettingItem item = new SettingItem();
        public SettingItem Item { get { return item; } }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            item.VoMan = (bool)chbMan.IsChecked;
            item.VoWoman = (bool)chbWoman.IsChecked;
            item.Artist = txbArtist.Text;
            item.Lang = txbLang.Text;
            item.Kind = cmbKind.Text;
            item.Reject = (bool)chbReject.IsChecked;
            if (item.Kind.Equals("All Artist"))
            {
                item.Album = "*";
            }
            else
            {
                item.Album = txbAlbum.Text;
            }
            DialogResult = true;
        }
        public void setTrack(IITTrack track)
        {
            txbTitle.Text = track.Name;
            txbAlbum.Text = track.Album;
            txbArtist.Text = track.Artist;
            txbGrouping.Text = track.Grouping;
            txbComment.Text = track.Comment;
            if (track is IITFileOrCDTrack)
            {
                txbLyrics.Text = (track as IITFileOrCDTrack).Lyrics;
            }

            if (!String.IsNullOrEmpty(track.Comment))
            {
                if (track.Comment.Contains(SettingItem.TAG_MAN))
                {
                    chbMan.IsChecked = true;
                }
                if (track.Comment.Contains(SettingItem.TAG_WOMAN))
                {
                    chbWoman.IsChecked = true;
                }
                if (track.Comment.Contains(SettingItem.TAG_REJECT))
                {
                    chbReject.IsChecked = true;
                }
                int index = track.Comment.IndexOf(SettingItem.TAG_LANG);
                if (index > 0)
                {
                    txbLang.Text = track.Comment.Substring(index + SettingItem.TAG_LANG.Length, 2);
                }
            }
            if (chbReject.IsChecked==null && !String.IsNullOrEmpty(track.Grouping))
            {
                if (track.Grouping.Contains("reject"))
                {
                    chbReject.IsChecked = true;
                }
            }
            try
            {
                track.Play();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
