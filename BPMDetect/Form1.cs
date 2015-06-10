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
            chart2.Series["bpm"].Points.Clear();
            chart2.ChartAreas["ChartArea1"].AxisX.Minimum = bpmDetector.BpmLow;
            chart2.ChartAreas["ChartArea1"].AxisX.Maximum = bpmDetector.BpmHigh;
            for (int bpm = bpmDetector.BpmLow; bpm <= bpmDetector.BpmHigh; bpm++)
            {
                chart2.Series["bpm"].Points.AddXY(bpm, bpmDetector.getBpmValue(bpm));
                bpm++;
            }

            chart2.Series["peak"].Points.Clear();
            for (int i = 0; i < 3 & i < bpmDetector.Peaks.Count; i++)
            {
                int bpm = bpmDetector.Peaks[i];
                double val = bpmDetector.getBpmValue(bpm);
                double pc = val / bpmDetector.getBpmValue(bpmDetector.Peaks[0]);
                txConsole.AppendText(String.Format("{0}:{1}({2})", bpm, val, pc) + Environment.NewLine);
                chart2.Series["peak"].Points.AddXY(bpm, val);
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
                BpmDetector bpmDetector = new BpmDetector();
                showSourceFormat(bpmDetector.WaveSource);
                writeConsole("bpm", bpmDetector.detect(openFileDialog.FileName));
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

        void listItemBpmDetect(ListViewItem item, BpmDetector bpmDetector)
        {
            IITFileOrCDTrack track = item.Tag as IITFileOrCDTrack;
            lbTitle.Text = track.Name;
            int bpm = bpmDetector.detect(track.Location);
            item.SubItems[4].Text = bpm.ToString();

            txConsole.AppendText(item.Name + Environment.NewLine);
            showSourceFormat(bpmDetector.WaveSource);
            writeConsole("bpm", bpm);
            showBpmChart(bpmDetector);

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
                ListViewItem item = lviTuneTracks.SelectedItems[0];
                IITFileOrCDTrack track = item.Tag as IITFileOrCDTrack;
                lbTitle.Text = track.Name;
                BpmDetector bpmDetector = createDetector();
                int bpm = bpmDetector.detect(track.Location);
                item.SubItems[4].Text = bpm.ToString();
                showSourceFormat(bpmDetector.WaveSource);
                writeConsole("bpm", bpm);
                showBpmChart(bpmDetector);
            }
        }
        bool bstop = false;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            BpmDetector bpmDetector = createDetector();

            toolStripButton1.Enabled = false;
            btListStop.Enabled = true;
            bstop = false;
            toolStripProgressBar1.Maximum = lviTuneTracks.Items.Count;
            foreach (ListViewItem item in lviTuneTracks.Items)
            {
                if (bstop) { break; }
                try
                {
                    item.EnsureVisible();

                    listItemBpmDetect(item, bpmDetector);
                    toolStripProgressBar1.PerformStep();
                    Application.DoEvents();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.StackTrace);
                }
            }
            toolStripButton1.Enabled = true;
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
                        lviTuneTracks.Items.Clear();
                        foreach (IITTrack track in pl.Tracks)
                        {
                            ListViewItem item = lviTuneTracks.Items.Add(track.Name);
                            item.SubItems.Add(track.Artist);
                            item.SubItems.Add(track.Album);
                            item.SubItems.Add(track.BPM.ToString());
                            item.SubItems.Add("-");
                            item.Tag = track;
                        }
                    }
                    finally
                    {
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
            foreach (ListViewItem item in lviTuneTracks.Items)
            {
                int bpm;
                if (int.TryParse(item.SubItems[4].Text, out bpm))
                {
                    IITTrack track = item.Tag as IITTrack;
                    if (track != null)
                    {
                        try
                        {
                            track.BPM = bpm;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.WriteLine(ex.StackTrace);
                        }
                    }
                }

            }

        }


    }
}
