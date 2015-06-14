using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using iTunesLib;

namespace BpmDetectorw
{
    public class PlaylistTreeItem
    {
        public static void createPlaylistTree(TreeView treeView,IITSource source)
        {
            List<PlaylistTreeItem> list = new List<PlaylistTreeItem>();
            foreach (IITPlaylist p in source.Playlists)
            {
                PlaylistTreeItem item = new PlaylistTreeItem() { Title = p.Name, iTunesPlaylist = p };
                list.Add(item);
            }

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

        public PlaylistTreeItem()
        {
            this.Items = new ObservableCollection<PlaylistTreeItem>();
        }
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
        public string Title { get; set; }
        public IITPlaylist iTunesPlaylist { get; set; }
        public ObservableCollection<PlaylistTreeItem> Items { get; set; }
    }
}
