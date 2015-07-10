using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using iTunesLib;
using SoundAnalyzeLib;

namespace BpmDetector.TreeList
{
    /// <summary>
    /// iTunesのプレイリストをツリー構造オブジェクトとして保持する
    /// </summary>
    public class PlaylistTreeItem
    {
        /// <summary>
        /// ツリー構造を生成するスタティックメソッド
        /// </summary>
        /// <param name="treeView">親のツリーアイテム</param>
        /// <param name="source">プレイリストが格納されているソース</param>
        /// <param name="dictionary">BPM検出クラスを格納しておくリスト</param>
        public static void createPlaylistTree(TreeView treeView, IITSource source, Dictionary<int, IBpmDetector> dictionary, string dataPath, string ext)
        {
            //まずはプレイリストの一覧を作る
            List<PlaylistTreeItem> list = new List<PlaylistTreeItem>();
            foreach (IITPlaylist p in source.Playlists)
            {
                PlaylistTreeItem item = new PlaylistTreeItem()
                {
                    Title = p.Name,
                    iTunesPlaylist = p,
                    Tracks = new TrackCollectionWrapper(p.Tracks, dictionary, dataPath, ext)
                };
                list.Add(item);
            }

            //ツリー構造にする
            foreach (PlaylistTreeItem item in list)
            {
                IITUserPlaylist userPlaylist = item.iTunesPlaylist as IITUserPlaylist;

                IITUserPlaylist parent = null;
                PlaylistTreeItem parentItem = null;
                if (userPlaylist != null && (parent = userPlaylist.get_Parent()) != null)
                {
                    parentItem = list.Find(x => x.iTunesPlaylist.playlistID.Equals(parent.playlistID));
                }
                if (parentItem == null)
                {
                    treeView.Items.Add(item);
                }
                else
                {
                    parentItem.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PlaylistTreeItem()
        {
            this.Items = new ObservableCollection<PlaylistTreeItem>();
        }

        /// <summary>
        /// プレイリストが格納されているアイテムを検索する
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        public PlaylistTreeItem findItem(IITPlaylist playlist)
        {
            foreach (PlaylistTreeItem item in Items)
            {
                if (item.iTunesPlaylist.playlistID.Equals(playlist.playlistID))
                {
                    return item;
                }
                if (item.Items.Count > 0)
                {
                    return item.findItem(playlist);
                }
            }
            return null;
        }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// プレイリスト
        /// </summary>
        public IITPlaylist iTunesPlaylist { get; set; }
        //        public ObservableTrackCollection Tracks { get; set; }

        /// <summary>
        /// 子要素はプロパティ変更が受け取れるやつで
        /// </summary>
        public ObservableCollection<PlaylistTreeItem> Items { get; set; }

        /// <summary>
        /// リストビュー用のコレクション
        /// </summary>
        public TrackCollectionWrapper Tracks { get; set; }
    }
}
