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

namespace TagSetter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {


        iTunesApp _app;
        IITTrackCollection _tracks;
        DefaultSet defset;
        const String defFileName = @"c:\temp\tagSetter.xml";
        public MainWindow()
        {
            InitializeComponent();
            _app = new iTunesApp();
            _tracks = _app.LibraryPlaylist.Tracks;
            lvTracks.ItemsSource = _tracks;
            defset = new DefaultSet();
            if (System.IO.File.Exists(defFileName))
            {
                defset.loadFromFile(defFileName);
            }
        }
        void sample()
        {
            foreach (IITTrack t in _tracks)
            {
                //t.Grouping
            }
        }
        SettingItem findItem(IITTrack track)
        {
            foreach (SettingItem item in defset.list)
            {
                if (item.Artist.Equals(track.Artist))
                {
                    if (item.Album.Equals("*"))
                    {
                        return item;
                    }
                    else if (item.Album.Equals(track.Album))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        String getComment(SettingItem item)
        {
            StringBuilder sb = new StringBuilder();
            if (item.VoMan)
            {
                sb.Append(SettingItem.TAG_MAN);
            }
            if (item.VoWoman)
            {
                sb.Append(SettingItem.TAG_WOMAN);
            }
            if (item.Reject)
            {
                sb.Append(SettingItem.TAG_REJECT);
            }
            if (!String.IsNullOrEmpty(item.Lang))
            {
                sb.Append(SettingItem.TAG_LANG + item.Lang + "]");
            }
            return sb.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (IITTrack t in _tracks)
            {
                if (!String.IsNullOrEmpty(t.Comment))
                {
                    if (t.Comment.Contains(SettingItem.TAG_MAN)
                        || t.Comment.Contains(SettingItem.TAG_WOMAN)
                        || t.Comment.Contains(SettingItem.TAG_LANG)
                        || t.Comment.Contains(SettingItem.TAG_REJECT))
                    {
                        continue;
                    }
                }

                lvTracks.SelectedIndex = t.Index - 1;
                lvTracks.ScrollIntoView(lvTracks.SelectedItem);
                SettingItem item = findItem(t);
                if (item == null)
                {
                    AskDialog dlg = new AskDialog(t);
                    Nullable<bool> result = dlg.ShowDialog();
                    if (result == true)
                    {
                        item = dlg.Item;
                        if (!item.Kind.Equals("Ask"))
                        {
                            defset.list.Add(item);
                        }
                    }
                    else { break; }
                }
                t.Comment = getComment(item);
                Console.WriteLine(t.Artist + "/" + t.Album + "/" + t.Name + ":" + getComment(item));
            }
            _app.Stop();
            defset.saveToFile(defFileName);
            /*DefaultSet s = new DefaultSet();
            SettingItem item=new SettingItem();
            item.Kind="all";
            item.Name="The Birthday Massacre";
            item.VoWoman=true;
            item.VoMan=false;
            item.Reject=false;
            item.Lang="en";
            s.list.Add(item);
            s.list.Add(new SettingItem() { Kind = "ask", Lang = "ja", Name = "陰陽座", Reject = false, VoMan = true, VoWoman = true });
            s.saveToFile(@"c:\temp\set.xml");
             */
        }

        private void lvTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IITTrack track = _tracks[lvTracks.SelectedIndex + 1];
            AskDialog dlg = new AskDialog(track);
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                track.Comment = getComment(dlg.Item);
                System.ComponentModel.ICollectionView view = CollectionViewSource.GetDefaultView(lvTracks.ItemsSource);
                view.Refresh();
            }
            _app.Stop();
        }
    }
    public class CommentComverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String comment = (String)value;
            if (SettingItem.VO_WOMAN.Equals(parameter))
            {
                return !String.IsNullOrEmpty(comment) && comment.Contains(SettingItem.TAG_WOMAN);
            }
            else if (SettingItem.VO_MAN.Equals(parameter))
            {
                return !String.IsNullOrEmpty(comment) && comment.Contains(SettingItem.TAG_MAN);
            }
            else if (SettingItem.REJECT.Equals(parameter))
            {
                return !String.IsNullOrEmpty(comment) && comment.Contains(SettingItem.TAG_REJECT);
            }
            else if (SettingItem.LANG.Equals(parameter))
            {
                if (!String.IsNullOrEmpty(comment) && comment.Contains(SettingItem.TAG_LANG))
                {
                    int index = comment.IndexOf(SettingItem.TAG_LANG);
                    return comment.Substring(index, SettingItem.TAG_LANG.Length + 3);
                }
                return "";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
