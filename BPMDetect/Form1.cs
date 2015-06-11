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
        class BpmListItem
        {
            public IITTrack track;
            public int detectBPM;
        }
        List<BpmListItem> list = new List<BpmListItem>();
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
            _itunesApp = new iTunesApp();
            tvPlayList.Nodes.Clear();
            foreach (IITPlaylist playList in _itunesApp.LibrarySource.Playlists)
            {
                TreeNode node = tvPlayList.Nodes.Add(playList.Name);
                node.Tag = playList;
            }

        }

        void listItemBpmDetect(BpmListItem item, BpmDetector bpmDetector)
        {
            IITFileOrCDTrack track = item.track as IITFileOrCDTrack;
            lbTitle.Text = track.Name;
            item.detectBPM = bpmDetector.detect(track.Location);

            txConsole.AppendText(item.track.Name + Environment.NewLine);
            showSourceFormat(bpmDetector.WaveSource);
            writeConsole("bpm", item.detectBPM);
            showBpmChart(bpmDetector);
            lviTuneTracks.Refresh();

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
                BpmListItem item = list[lviTuneTracks.SelectedIndices[0]];
                IITFileOrCDTrack track = item.track as IITFileOrCDTrack;
                lbTitle.Text = track.Name;
                BpmDetector bpmDetector = createDetector();
                int bpm = bpmDetector.detect(track.Location);
                item.detectBPM = bpm;
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
            foreach (var bpmObj in list.Select((value, index) => new { value, index }))
            {
                if (bstop) { break; }
                if (bpmObj.value.detectBPM == 0)
                {
                    try
                    {
                        listItemBpmDetect(bpmObj.value, bpmDetector);
                        toolStripProgressBar1.PerformStep();
                        lviTuneTracks.Items[bpmObj.index].EnsureVisible();
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
                IITPlaylist pl = e.Node.Tag as IITPlaylist;
                if (pl != null)
                {

                    lviTuneTracks.BeginUpdate();
                    try
                    {
                        list.Clear();
                        foreach (IITTrack track in pl.Tracks)
                        {
                            BpmListItem item = new BpmListItem();
                            item.track = track;
                            item.detectBPM = 0;
                            list.Add(item);
                        }
                    }
                    finally
                    {
                        lviTuneTracks.VirtualListSize = list.Count;
                        lviTuneTracks.EndUpdate();
                    }
                }
            }
        }

        private void btListStop_Click(object sender, EventArgs e)
        {
            bstop = true;
        }

        private void btWrite_Click(object sender, EventArgs e)
        {
            foreach (BpmListItem item in list)
            {
                if (item.detectBPM > 0 && item.detectBPM != item.track.BPM)
                {
                    try
                    {
                        item.track.BPM = item.detectBPM;
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
            if (e.ItemIndex < list.Count)
            {
                BpmListItem bpmItem = list[e.ItemIndex];
                e.Item = new ListViewItem(bpmItem.track.Name);
                e.Item.SubItems.Add(bpmItem.track.Artist);
                e.Item.SubItems.Add(bpmItem.track.Album);
                e.Item.SubItems.Add(bpmItem.track.BPM.ToString());
                if (bpmItem.detectBPM == 0)
                {
                    e.Item.SubItems.Add("-");
                }
                else
                {
                    e.Item.SubItems.Add(bpmItem.detectBPM.ToString());
                }
            }
        }
        Random _random = null;

        List<IITTrack> createRandomList(List<IITTrack> trackList, int min, int max)
        {
            if (_random == null)
            {
                _random = new Random();
            }
            List<IITTrack> list = new List<IITTrack>();
            int du = 0;
            while (du < min * 60 || du > max * 60)
            {
                list.Clear();
                du = 0;
                while (du < min)
                {
                    IITTrack track = trackList[_random.Next(trackList.Count)];
                    if (!list.Exists(x => x.Equals(track)))
                    {
                        list.Add(track);
                        du += track.Duration;
                    }
                }
            }
            return list;

        }
        private void btCreatePlayList_Click(object sender, EventArgs e)
        {
            IITLibraryPlaylist mainPlayList = _itunesApp.LibraryPlaylist;
            List<IITTrack> trackList = new List<IITTrack>();
            foreach (string line in textBox1.Lines)
            {
                if (line.StartsWith("#") || line.StartsWith("\\")) { continue; }
                else
                {
                    string[] args = line.Split(' ');
                    try
                    {
                        int bpmFrom = int.Parse(args[0]);
                        int bpmTo = int.Parse(args[1]);
                        int minuteMin = int.Parse(args[2]);
                        int minuteMax = int.Parse(args[3]);
                        foreach (IITTrack track in mainPlayList.Tracks)
                        {
                            if (track.BPM >= bpmFrom && track.BPM <= bpmTo)
                            {
                                trackList.Add(track);
                            }
                        }
                        foreach (IITTrack track in createRandomList(trackList, minuteMin, minuteMax))
                        {
                            ListViewItem listViewItem = lvPlayList.Items.Add(track.Name);
                            listViewItem.SubItems.Add(track.Artist);
                            listViewItem.SubItems.Add(track.Album);
                            listViewItem.SubItems.Add(track.BPM.ToString());
                            listViewItem.SubItems.Add(track.Time);

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

    }
}
