using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams;
using iTunesLib;

namespace BPMDetect
{
    public partial class Form1 : Form
    {
        Dictionary<int, int> listDetectBpm = new Dictionary<int, int>();
        IITPlaylist currentPlaylist = null;
        public Form1()
        {
            InitializeComponent();
        }
        void writeConsole(String s)
        {
            txConsole.AppendText(s + System.Environment.NewLine);
        }
        void writeConsole(String s, int value)
        {
            writeConsole(String.Format("{0}:{1}", s, value));
        }
        void showSourceFormat(IWaveSource ws)
        {
            writeConsole("Channels", ws.WaveFormat.Channels);
            writeConsole("BitsPerSample", ws.WaveFormat.BitsPerSample);
            writeConsole("BlockAlign", ws.WaveFormat.BlockAlign);
            writeConsole("BytesPerBlock", ws.WaveFormat.BytesPerBlock);
            writeConsole("BytesPerSample", ws.WaveFormat.BytesPerSample);
            writeConsole("BytesPerSecond", ws.WaveFormat.BytesPerSecond);
            writeConsole("ExtraSize", ws.WaveFormat.ExtraSize);
            writeConsole("SampleRate", ws.WaveFormat.SampleRate);
            writeConsole("Length", (int)ws.Length);
        }
        void showBpmChart(BpmDetector bpmDetector)
        {
            charBPM.Series["bpm"].Points.Clear();
            charBPM.ChartAreas["ChartArea1"].AxisX.Minimum = bpmDetector.BpmLow;
            charBPM.ChartAreas["ChartArea1"].AxisX.Maximum = bpmDetector.BpmHigh;
            for (int bpm = bpmDetector.BpmLow; bpm <= bpmDetector.BpmHigh; bpm++)
            {
                charBPM.Series["bpm"].Points.AddXY(bpm, bpmDetector.getBpmValue(bpm));
                bpm++;
            }

            charBPM.Series["peak"].Points.Clear();
            for (int i = 0; i < 3 & i < bpmDetector.Peaks.Count; i++)
            {
                int bpm = bpmDetector.Peaks[i];
                double val = bpmDetector.getBpmValue(bpm);
                double pc = val / bpmDetector.getBpmValue(bpmDetector.Peaks[0]);
                txConsole.AppendText(String.Format("{0}:{1}({2})", bpm, val, pc) + Environment.NewLine);
                charBPM.Series["peak"].Points.AddXY(bpm, val);
            }
        }

        private void btOpen_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = CodecFactory.SupportedFilesFilterEn,
                Title = "Select a file..."
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                BpmDetector bpmDetector = createDetector();
                writeConsole("bpm", bpmDetector.detect(openFileDialog.FileName));
                showSourceFormat(bpmDetector.WaveSource);
                showBpmChart(bpmDetector);
            }
        }

        iTunesApp _itunesApp;
        private void btiTunes_Click(object sender, EventArgs e)
        {
            openiTunes();
        }
        void openiTunes()
        {
            if (_itunesApp != null) return;
            _itunesApp = new iTunesApp();
            tvPlayList.Nodes.Clear();
            foreach (IITPlaylist playList in _itunesApp.LibrarySource.Playlists)
            {
                TreeNode node = tvPlayList.Nodes.Add(playList.Name);
                node.Tag = playList;
                IITUserPlaylist upl = playList as IITUserPlaylist;
                if (upl != null ){
                    IITUserPlaylist parent =upl.get_Parent();
                    if (parent != null)
                    {
                        foreach (TreeNode n in tvPlayList.Nodes)
                        {
                            IITPlaylist ppl = n.Tag as IITPlaylist;
                            
                            if (ppl!=null && ppl.playlistID.Equals(parent.playlistID))
                            {
                                if (node.Level == 0)
                                {
                                    tvPlayList.Nodes.Remove(node);
                                }
                                else
                                {
                                    node.Parent.Nodes.Remove(node);
                                }
                                n.Nodes.Add(node);
                                break;
                            }
                        }
                    }

                }
            }

        }

        int listItemBpmDetect(IITFileOrCDTrack track, BpmDetector bpmDetector)
        {
            lbTitle.Text = track.Name;
            int bpm = bpmDetector.detect(track.Location);
            if (listDetectBpm.ContainsKey(track.trackID))
            {
                listDetectBpm[track.trackID] = bpm;
            }
            else
            {
                listDetectBpm.Add(track.trackID, bpm);
            }
            txConsole.AppendText(track.Name + Environment.NewLine);
            showSourceFormat(bpmDetector.WaveSource);
            writeConsole("bpm", bpm);
            showBpmChart(bpmDetector);
            lviTuneTracks.Refresh();
            return bpm;

        }
        BpmDetector createDetector()
        {
            BpmDetector detector = new BpmDetector();
            foreach (string s in txBpmDetectSettings.Lines)
            {
                string[] arg = s.Split(' ');
                if (arg.Length > 1)
                {
                    int val;
                    double th;
                    if (arg[0].Equals("bpm_low") && (int.TryParse(arg[1], out val))) { detector.BpmLow = val; }
                    if (arg[0].Equals("bpm_high") && ((int.TryParse(arg[1], out val)))) { detector.BpmHigh = val; }
                    if (arg[0].Equals("prio_low") && (int.TryParse(arg[1], out val))) { detector.BriorityBpmLow = val; }
                    if (arg[0].Equals("prio_high") && (int.TryParse(arg[1], out val))) { detector.BriorityBpmHigh = val; }
                    if (arg[0].Equals("window_size") && (int.TryParse(arg[1], out val))) { detector.WindowSize = val; }
                    if (arg[0].Equals("threshold") && (double.TryParse(arg[1], out th))) { detector.Threshold = th; }
                }
            }
            return detector;
        }


        private void btiTunes_DoubleClick(object sender, EventArgs e)
        {
            if (lviTuneTracks.SelectedItems != null)
            {
                int index = lviTuneTracks.SelectedIndices[0] + 1;
                IITFileOrCDTrack track = currentPlaylist.Tracks[index] as IITFileOrCDTrack;
                lbTitle.Text = track.Name;
                BpmDetector bpmDetector = createDetector();
                int bpm = bpmDetector.detect(track.Location);
                if (listDetectBpm.ContainsKey(track.trackID))
                {
                    listDetectBpm[track.trackID] = bpm;
                }
                else
                {
                    listDetectBpm.Add(track.trackID, bpm);
                }
                showSourceFormat(bpmDetector.WaveSource);
                writeConsole("bpm", bpm);
                showBpmChart(bpmDetector);
                lviTuneTracks.Refresh();
            }
        }
        bool bstop = false;
        private void btDoDetect_Click(object sender, EventArgs e)
        {
            BpmDetector bpmDetector = createDetector();

            btDoDetect.Enabled = false;
            btListStop.Enabled = true;
            bstop = false;
            toolStripProgressBar1.Maximum = lviTuneTracks.Items.Count;
            for (int i = 0; i < currentPlaylist.Tracks.Count; i++)
            {
                if (bstop) { break; }
                IITFileOrCDTrack track = currentPlaylist.Tracks[i + 1] as IITFileOrCDTrack;
                if (track != null && !listDetectBpm.ContainsKey(track.trackID))
                {
                    try
                    {
                        listItemBpmDetect(track, bpmDetector);
                        toolStripProgressBar1.PerformStep();
                        lviTuneTracks.Items[i].EnsureVisible();
                        Application.DoEvents();
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.StackTrace);
                    }
                }
            }
            btDoDetect.Enabled = true;
            btListStop.Enabled = false;
            toolStripProgressBar1.Value = 0;
        }

        private void tvPlayList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null)
            {
                currentPlaylist = e.Node.Tag as IITPlaylist;
                if (currentPlaylist != null)
                {
                    lviTuneTracks.VirtualListSize = currentPlaylist.Tracks.Count;
                }
            }
        }

        private void btListStop_Click(object sender, EventArgs e)
        {
            bstop = true;
        }

        private void btWrite_Click(object sender, EventArgs e)
        {
            foreach (IITFileOrCDTrack track in currentPlaylist.Tracks)
            {

                if (listDetectBpm.ContainsKey(track.trackID))
                {
                    try
                    {
                        track.BPM = listDetectBpm[track.trackID];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }

            }

        }

        private void lviTuneTracks_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex < currentPlaylist.Tracks.Count)
            {
                IITTrack track = currentPlaylist.Tracks[e.ItemIndex + 1];
                e.Item = new ListViewItem(track.Name);
                e.Item.SubItems.Add(track.Artist);
                e.Item.SubItems.Add(track.Album);
                e.Item.SubItems.Add(track.BPM.ToString());
                if (listDetectBpm.ContainsKey(track.trackID))
                {
                    e.Item.SubItems.Add(listDetectBpm[track.trackID].ToString());
                }
                else
                {
                    e.Item.SubItems.Add("-");
                }
            }
        }
        Random _random = null;

        List<IITTrack> createRandomList(IITPlaylist playlist, int minMinute, int maxMinute)
        {
            if (_random == null)
            {
                _random = new Random();
            }
            List<IITTrack> list = new List<IITTrack>();
            int minSec = minMinute * 60;
            int maxSec = maxMinute * 60;
            int du = 0;
            while (du < minSec || du > maxSec)
            {
                list.Clear();
                du = 0;
                while (du < minSec)
                {
                    IITTrack track = playlist.Tracks[_random.Next(playlist.Tracks.Count) + 1];
                    if (!list.Exists(x => x.Equals(track)))
                    {
                        list.Add(track);
                        du += track.Duration;
                    }
                }
            }
            return list;

        }
        private void btGeneratePlayList_Click(object sender, EventArgs e)
        {
            openiTunes();
            IITLibraryPlaylist mainPlayList = _itunesApp.LibraryPlaylist;
            List<IITTrack> trackList = new List<IITTrack>();
            lvPlayList.Items.Clear();
            int du = 0;
            foreach (string line in txPlaylistSettings.Lines)
            {
                if (line.StartsWith("#") || line.StartsWith("\\")) { continue; }
                else
                {
                    string[] args = line.Split(' ');
                    try
                    {
                        IITPlaylist playlist = _itunesApp.LibrarySource.Playlists.get_ItemByName(args[0]);

                        int minuteMin = int.Parse(args[1]);
                        int minuteMax = int.Parse(args[2]);
                        List<IITTrack> list = createRandomList(playlist, minuteMin, minuteMax);

                        foreach (IITTrack track in list)
                        {
                            ListViewItem listViewItem = lvPlayList.Items.Add(track.Name);
                            listViewItem.SubItems.Add(track.Artist);
                            listViewItem.SubItems.Add(track.Album);
                            listViewItem.SubItems.Add(track.BPM.ToString());
                            listViewItem.SubItems.Add(track.Time);
                            listViewItem.Tag = track;
                            du += track.Duration;

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            TimeSpan t = TimeSpan.FromSeconds(du);
            lbTitle.Text = t.ToString();
        }

        private void btWritePlaylistToiTunes_Click(object sender, EventArgs e)
        {
            foreach (string s in txPlaylistSettings.Lines)
            {
                if (s.StartsWith("\\"))
                {
                    string[] args = s.Split('\\');

                    IITPlaylist parent = _itunesApp.LibrarySource.Playlists.get_ItemByName(args[1]);
                    IITPlaylist playlist = _itunesApp.LibrarySource.Playlists.get_ItemByName(args[2]);
                    if (playlist != null)
                    {
                        playlist.Delete();
                    }
                    IITUserPlaylist upl = _itunesApp.CreatePlaylist(args[2]) as IITUserPlaylist;
                    upl.set_Parent(parent);
                    for (int i = 0; i < lvPlayList.Items.Count; i++)
                    {
                        ListViewItem item = lvPlayList.Items[i];
                        IITTrack track = item.Tag as IITTrack;
                        upl.AddTrack(track);
                    }
                    break;
                }
            }
        }

    }
}
