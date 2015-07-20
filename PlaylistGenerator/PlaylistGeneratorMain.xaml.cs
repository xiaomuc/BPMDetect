using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using iTunesLib;

namespace PlaylistGenerator
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistGeneratorMain : Window
    {
        #region fields
        /// <summary>
        /// iTunes書き込みプレイリスト名に使用する区切り文字
        /// </summary>
        char[] SPLIT_CHAR = { '\\', '/' };

        /// <summary>
        /// ターゲット時間の幅
        /// </summary>
        int DURATION_WIDTH = 60;

        /// <summary>
        /// プレイリスト自動生成のリトライ回数
        /// </summary>
        int MAX_RETRY = 10;

        /// <summary>
        /// プレイリスト生成ソースが改変されたかどうか
        /// </summary>
        bool _modified = false;

        /// <summary>
        /// iTunesアプリケーション
        /// </summary>
        iTunesApp _app;

        /// <summary>
        /// プレイリスト生成ソースリスト
        /// </summary>
        GenerateSourceList _generateSourceList;

        /// <summary>
        /// iTunesのプレイリスト名
        /// </summary>
        List<string> _playlistNames;

        /// <summary>
        /// 生成したトラックリスト
        /// </summary>
        List<IITTrack> _tracklist;
        
        #endregion

        #region constructor
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlaylistGeneratorMain()
        {
            InitializeComponent();

            //iTunes接続
            _app = new iTunesApp();

            //iTunesのプレイリスト名を取得してコンボボックスに設定
            _playlistNames = new List<string>();
            foreach (IITPlaylist pl in _app.LibrarySource.Playlists)
            {
                _playlistNames.Add(pl.Name);
            }
            cmbPlaylist.ItemsSource = _playlistNames;

            //生成ソースリストをつくる
            if (File.Exists(Properties.Settings.Default.CodeFileName))
            {
                loadGeneraterList(Properties.Settings.Default.CodeFileName);
            }
            else
            {
                _generateSourceList = new GenerateSourceList();
                _generateSourceList.Items.CollectionChanged += GenerateSourceItems_CollectionChanged;
                lvCode.ItemsSource = _generateSourceList.Items;
            }
            txbPlaylistName.Text = Properties.Settings.Default.iTunesPlaylistName;
        }
        #endregion

        #region private method
        /// <summary>
        /// 生成ソースをファイルから読み込む
        /// </summary>
        /// <param name="fileName"></param>
        void loadGeneraterList(string fileName)
        {
            try
            {
                if (_generateSourceList != null)
                {
                    _generateSourceList.Items.CollectionChanged -= GenerateSourceItems_CollectionChanged;
                }
                XmlSerializer serializer = new XmlSerializer(typeof(GenerateSourceList));
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    _generateSourceList = (GenerateSourceList)serializer.Deserialize(fs);
                    fs.Close();
                }
                _generateSourceList.Items.CollectionChanged += GenerateSourceItems_CollectionChanged;
                lvCode.ItemsSource = _generateSourceList.Items;
                lvCode.Items.Refresh();
                Properties.Settings.Default.CodeFileName = fileName;
                GenerateSourceItems_CollectionChanged(this, new System.Collections.Specialized.NotifyCollectionChangedEventArgs(System.Collections.Specialized.NotifyCollectionChangedAction.Add,_generateSourceList.Items));
                _modified = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 生成ソースをファイルに保存
        /// </summary>
        void saveGeneratorlist()
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.FileName = Properties.Settings.Default.CodeFileName;
            bool? result = saveDialog.ShowDialog();
            if (result == true)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GenerateSourceList));
                using (FileStream fs = new FileStream(saveDialog.FileName, FileMode.Create))
                {
                    serializer.Serialize(fs, _generateSourceList);
                    fs.Close();
                }
                Properties.Settings.Default.CodeFileName = saveDialog.FileName;
                _modified = false;
            }
        }

        /// <summary>
        /// 保存するプレイリスト名称を取得する
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parent"></param>
        /// <param name="target"></param>
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
            else
            {
                target = value;
            }
        }

        /// <summary>
        /// iTunesにプレイリスト書込する前に、古いやつを削除
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        void deleteExistPlaylist(string parent, string name)
        {
            foreach (IITPlaylist pl in _app.LibrarySource.Playlists)
            {
                if (pl.Kind == ITPlaylistKind.ITPlaylistKindUser)
                {
                    if (pl.Name.Equals(name))
                    {
                        IITUserPlaylist pr = ((IITUserPlaylist)pl).get_Parent();
                        if (pr == null && string.IsNullOrEmpty(parent))
                        {
                            pl.Delete();
                        }
                        if (pr != null && pr.Name.Equals(parent))
                        {
                            pl.Delete();
                        }
                    }
                }
            }
        }
        #endregion

        #region event handler
        /// <summary>
        /// 生成ソースリストの変更イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GenerateSourceItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            btnCreatePlaylist.IsEnabled =_generateSourceList.Items.Count > 0;
            btnSave.IsEnabled = _generateSourceList.Items.Count > 0;
            btnNew.IsEnabled = _generateSourceList.Items.Count > 0;
            sbiSourceDuration.Content = _generateSourceList.Items.Sum(x => x.Duration).ToString();
            _modified = true;
            lvCode.Items.Refresh();
        }


        /// <summary>
        /// 開くボタン。生成ソースをファイルから読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 保存ボタン。生成ソースの保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            saveGeneratorlist();
        }

        /// <summary>
        /// 追加ボタン。生成ソースに行を追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            GenerateSource g = new GenerateSource()
            {
                Playlist = (string)cmbPlaylist.SelectedValue,
                Duration = (int)iudDuration.Value
            };
            _generateSourceList.Items.Add(g);
        }

        /// <summary>
        /// 削除ボタン。生成ソースの1行を削除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            GenerateSource c = lvCode.SelectedItem as GenerateSource;
            if (c != null)
            {
                _generateSourceList.Items.Remove(c);
            }
        }

        /// <summary>
        /// 生成ソースリストの選択アイテムが変わったらツールバーに反映する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvCode.SelectedItem != null)
            {
                GenerateSource g = (GenerateSource)lvCode.SelectedItem;
                cmbPlaylist.SelectedValue = g.Playlist;
                iudDuration.Value = g.Duration;
            }
            btnUpdate.IsEnabled = lvCode.SelectedItem != null;
            btnDelete.IsEnabled = lvCode.SelectedItem != null;
        }

        /// <summary>
        /// ツールバーの内容で現在選択されている生成ソース行を入れ替える
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (lvCode.SelectedItem != null)
            {
                GenerateSource gs = (GenerateSource)lvCode.SelectedItem;
                gs.Playlist = (string)cmbPlaylist.SelectedValue;
                gs.Duration = (int)iudDuration.Value;
            }
        }

        /// <summary>
        /// 生成ソースをすべてクリアして新規作成する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btNew_Click(object sender, RoutedEventArgs e)
        {
            _generateSourceList.Items.Clear();
        }

        /// <summary>
        /// プレイリスト生成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            _tracklist = new List<IITTrack>();
            Random ran = new Random();
            foreach (GenerateSource g in _generateSourceList.Items)
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
                int maxDurationSec = g.Duration * 60 + DURATION_WIDTH;
                int minDurationSec = g.Duration * 60 - DURATION_WIDTH;
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
                            maxDurationSec += DURATION_WIDTH / 2;
                            minDurationSec -= DURATION_WIDTH / 2;
                            counter = 0;
                        }
                    }
                }
                foreach (IITTrack track in list)
                {
                    _tracklist.Add(track);
                }
            }
            TimeSpan ts = new TimeSpan(0, 0, _tracklist.Sum(x => x.Duration));
            sbiPlaylistDuration.Content = ts.ToString();
            lvGeneratedPlaylist.ItemsSource = _tracklist;
            btnWriteToitunes.IsEnabled = _tracklist.Count > 0;
        }

        /// <summary>
        /// iTunes書き込みボタン処理。保存先プレイリスト名取得のためダイアログを表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWriteToitunes_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// デストラクタ代わり
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.iTunesPlaylistName = txbPlaylistName.Text;
            if (_modified)
            {
                saveGeneratorlist();
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// iTunesに書き込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWriteOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string parent, target;
                getSavePlaylist(txbPlaylistName.Text, out parent, out target);
                deleteExistPlaylist(parent, target);

                IITUserPlaylist plTarget = _app.CreatePlaylist(target) as IITUserPlaylist;
                if (!string.IsNullOrEmpty(parent))
                {
                    IITPlaylist plParent = _app.LibrarySource.Playlists.get_ItemByName(parent);
                    if (plParent == null)
                    {
                        plParent = _app.CreatePlaylist(parent);
                    }
                    plTarget.set_Parent(plParent);
                }
                foreach (IITTrack track in _tracklist)
                {
                    plTarget.AddTrack(track);
                }
                Properties.Settings.Default.iTunesPlaylistName = txbPlaylistName.Text;
            }
            finally { InputBox.Visibility = System.Windows.Visibility.Collapsed; }
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion
    }

    /// <summary>
    /// XML保存するためにリストをラップしている
    /// </summary>
    public class GenerateSourceList
    {
        public ObservableCollection<GenerateSource> Items = new ObservableCollection<GenerateSource>();
    }

    /// <summary>
    /// 生成ソースの1行に相当
    /// </summary>
    public class GenerateSource
    {
        public string Playlist { get; set; }
        public int Duration { get; set; }
    }
}
