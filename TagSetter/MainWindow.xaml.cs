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
                if (item.Name.Equals(track.Artist)) { return item; }
                if (item.Name.Equals(track.Album)) { return item; }
            }
            return null;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AskDialog dlg = new AskDialog();
            foreach (IITTrack t in _tracks)
            {
                SettingItem item = findItem(t);
                if (item == null)
                {
                    dlg.setTrack(t);
                    Nullable<bool> result = dlg.ShowDialog();
                    if (result == true)
                    {

                    }
                    else { break; }
                }
            }
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
    }
    public class CommentComverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String comment = (String)value;
            if ("w".Equals(parameter))
            {
                return !String.IsNullOrEmpty(comment) && comment.Contains("[w]");
            }
            else if ("m".Equals(parameter))
            {
                return !String.IsNullOrEmpty(comment) && comment.Contains("[m]");
            }
            else if ("r".Equals(parameter))
            {
                return !String.IsNullOrEmpty(comment) && comment.Contains("[r]");
            }
            else if ("l".Equals(parameter))
            {
                if (!String.IsNullOrEmpty(comment) && comment.Contains("[lang:"))
                {
                    int index = comment.IndexOf("[lang:");
                    return comment.Substring(index, 9);
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
