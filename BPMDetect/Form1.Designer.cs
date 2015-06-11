namespace BPMDetect
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint5 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(60D, 0D);
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint6 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(240D, 0D);
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint7 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(60D, 0D);
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint8 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(240D, 0D);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.lbTitle = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tbpBPMDetect = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvPlayList = new System.Windows.Forms.TreeView();
            this.txBpmDetectSettings = new System.Windows.Forms.TextBox();
            this.lviTuneTracks = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.charBPM = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tbpPlaylist = new System.Windows.Forms.TabPage();
            this.lvPlayList = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.txPlaylistSettings = new System.Windows.Forms.TextBox();
            this.tbpConsole = new System.Windows.Forms.TabPage();
            this.txConsole = new System.Windows.Forms.TextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btOpen = new System.Windows.Forms.ToolStripButton();
            this.btiTunes = new System.Windows.Forms.ToolStripButton();
            this.btDoDetect = new System.Windows.Forms.ToolStripButton();
            this.btListStop = new System.Windows.Forms.ToolStripButton();
            this.btWrite = new System.Windows.Forms.ToolStripButton();
            this.btGeneratePlaylist = new System.Windows.Forms.ToolStripButton();
            this.btWritePlaylistToiTunes = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tbpBPMDetect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.charBPM)).BeginInit();
            this.tbpPlaylist.SuspendLayout();
            this.tbpConsole.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.tabControl1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(793, 257);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(793, 304);
            this.toolStripContainer1.TabIndex = 0;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.lbTitle});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(793, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar1.Step = 1;
            // 
            // lbTitle
            // 
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(0, 17);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tbpBPMDetect);
            this.tabControl1.Controls.Add(this.tbpPlaylist);
            this.tabControl1.Controls.Add(this.tbpConsole);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(793, 257);
            this.tabControl1.TabIndex = 2;
            // 
            // tbpBPMDetect
            // 
            this.tbpBPMDetect.Controls.Add(this.splitContainer1);
            this.tbpBPMDetect.Location = new System.Drawing.Point(4, 22);
            this.tbpBPMDetect.Name = "tbpBPMDetect";
            this.tbpBPMDetect.Padding = new System.Windows.Forms.Padding(3);
            this.tbpBPMDetect.Size = new System.Drawing.Size(785, 231);
            this.tbpBPMDetect.TabIndex = 1;
            this.tbpBPMDetect.Text = "BPM Detector";
            this.tbpBPMDetect.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvPlayList);
            this.splitContainer1.Panel1.Controls.Add(this.txBpmDetectSettings);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.lviTuneTracks);
            this.splitContainer1.Panel2.Controls.Add(this.charBPM);
            this.splitContainer1.Size = new System.Drawing.Size(779, 225);
            this.splitContainer1.SplitterDistance = 145;
            this.splitContainer1.TabIndex = 2;
            // 
            // tvPlayList
            // 
            this.tvPlayList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvPlayList.Location = new System.Drawing.Point(0, 0);
            this.tvPlayList.Name = "tvPlayList";
            this.tvPlayList.Size = new System.Drawing.Size(145, 146);
            this.tvPlayList.TabIndex = 1;
            this.tvPlayList.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvPlayList_NodeMouseClick);
            // 
            // txBpmDetectSettings
            // 
            this.txBpmDetectSettings.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txBpmDetectSettings.Location = new System.Drawing.Point(0, 146);
            this.txBpmDetectSettings.Multiline = true;
            this.txBpmDetectSettings.Name = "txBpmDetectSettings";
            this.txBpmDetectSettings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txBpmDetectSettings.Size = new System.Drawing.Size(145, 79);
            this.txBpmDetectSettings.TabIndex = 2;
            this.txBpmDetectSettings.Text = "bpm_low 80\r\nbpm_high 200\r\nprio_low 110\r\nprio_high 150\r\nwindow_size 2048\r\nthreshol" +
    "d 0.5\r\n";
            this.txBpmDetectSettings.WordWrap = false;
            // 
            // lviTuneTracks
            // 
            this.lviTuneTracks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lviTuneTracks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lviTuneTracks.FullRowSelect = true;
            this.lviTuneTracks.Location = new System.Drawing.Point(0, 0);
            this.lviTuneTracks.Name = "lviTuneTracks";
            this.lviTuneTracks.Size = new System.Drawing.Size(630, 98);
            this.lviTuneTracks.TabIndex = 0;
            this.lviTuneTracks.UseCompatibleStateImageBehavior = false;
            this.lviTuneTracks.View = System.Windows.Forms.View.Details;
            this.lviTuneTracks.VirtualMode = true;
            this.lviTuneTracks.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.lviTuneTracks_RetrieveVirtualItem);
            this.lviTuneTracks.DoubleClick += new System.EventHandler(this.btiTunes_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Artist";
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Album";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "BPM(set)";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "BPM(Detect)";
            // 
            // charBPM
            // 
            chartArea2.AxisX.Interval = 10D;
            chartArea2.AxisX.Maximum = 220D;
            chartArea2.AxisX.Minimum = 80D;
            chartArea2.Name = "ChartArea1";
            this.charBPM.ChartAreas.Add(chartArea2);
            this.charBPM.Dock = System.Windows.Forms.DockStyle.Bottom;
            legend2.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend2.Name = "Legend1";
            this.charBPM.Legends.Add(legend2);
            this.charBPM.Location = new System.Drawing.Point(0, 98);
            this.charBPM.Name = "charBPM";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "bpm";
            series3.Points.Add(dataPoint5);
            series3.Points.Add(dataPoint6);
            series3.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "peak";
            series4.Points.Add(dataPoint7);
            series4.Points.Add(dataPoint8);
            series4.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            this.charBPM.Series.Add(series3);
            this.charBPM.Series.Add(series4);
            this.charBPM.Size = new System.Drawing.Size(630, 127);
            this.charBPM.TabIndex = 0;
            this.charBPM.Text = "chart2";
            // 
            // tbpPlaylist
            // 
            this.tbpPlaylist.Controls.Add(this.lvPlayList);
            this.tbpPlaylist.Controls.Add(this.txPlaylistSettings);
            this.tbpPlaylist.Location = new System.Drawing.Point(4, 22);
            this.tbpPlaylist.Name = "tbpPlaylist";
            this.tbpPlaylist.Size = new System.Drawing.Size(785, 231);
            this.tbpPlaylist.TabIndex = 2;
            this.tbpPlaylist.Text = "Playlist Generator";
            this.tbpPlaylist.UseVisualStyleBackColor = true;
            // 
            // lvPlayList
            // 
            this.lvPlayList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.lvPlayList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPlayList.Location = new System.Drawing.Point(225, 0);
            this.lvPlayList.Name = "lvPlayList";
            this.lvPlayList.Size = new System.Drawing.Size(560, 231);
            this.lvPlayList.TabIndex = 1;
            this.lvPlayList.UseCompatibleStateImageBehavior = false;
            this.lvPlayList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Name";
            this.columnHeader6.Width = 41;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Artist";
            this.columnHeader7.Width = 41;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Album";
            this.columnHeader8.Width = 44;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "BPM";
            this.columnHeader9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader9.Width = 36;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Time";
            this.columnHeader10.Width = 394;
            // 
            // txPlaylistSettings
            // 
            this.txPlaylistSettings.Dock = System.Windows.Forms.DockStyle.Left;
            this.txPlaylistSettings.Location = new System.Drawing.Point(0, 0);
            this.txPlaylistSettings.Multiline = true;
            this.txPlaylistSettings.Name = "txPlaylistSettings";
            this.txPlaylistSettings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txPlaylistSettings.Size = new System.Drawing.Size(225, 231);
            this.txPlaylistSettings.TabIndex = 0;
            this.txPlaylistSettings.Text = "#playlist min max\r\nbpm110 3 6\r\nbpm120 4 6\r\nbpm130 19 21\r\nbpm120 9 11\r\n\\次郎\\walk";
            this.txPlaylistSettings.WordWrap = false;
            // 
            // tbpConsole
            // 
            this.tbpConsole.Controls.Add(this.txConsole);
            this.tbpConsole.Location = new System.Drawing.Point(4, 22);
            this.tbpConsole.Name = "tbpConsole";
            this.tbpConsole.Padding = new System.Windows.Forms.Padding(3);
            this.tbpConsole.Size = new System.Drawing.Size(785, 231);
            this.tbpConsole.TabIndex = 0;
            this.tbpConsole.Text = "console";
            this.tbpConsole.UseVisualStyleBackColor = true;
            // 
            // txConsole
            // 
            this.txConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txConsole.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txConsole.Location = new System.Drawing.Point(3, 3);
            this.txConsole.Multiline = true;
            this.txConsole.Name = "txConsole";
            this.txConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txConsole.Size = new System.Drawing.Size(779, 225);
            this.txConsole.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btOpen,
            this.btiTunes,
            this.btDoDetect,
            this.btListStop,
            this.btWrite,
            this.btGeneratePlaylist,
            this.btWritePlaylistToiTunes});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(485, 25);
            this.toolStrip1.TabIndex = 0;
            // 
            // btOpen
            // 
            this.btOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btOpen.Image = ((System.Drawing.Image)(resources.GetObject("btOpen.Image")));
            this.btOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(42, 22);
            this.btOpen.Text = "Open";
            this.btOpen.Click += new System.EventHandler(this.btOpen_Click);
            // 
            // btiTunes
            // 
            this.btiTunes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btiTunes.Image = ((System.Drawing.Image)(resources.GetObject("btiTunes.Image")));
            this.btiTunes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btiTunes.Name = "btiTunes";
            this.btiTunes.Size = new System.Drawing.Size(49, 22);
            this.btiTunes.Text = "iTunes";
            this.btiTunes.Click += new System.EventHandler(this.btiTunes_Click);
            this.btiTunes.DoubleClick += new System.EventHandler(this.btiTunes_DoubleClick);
            // 
            // btDoDetect
            // 
            this.btDoDetect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btDoDetect.Image = ((System.Drawing.Image)(resources.GetObject("btDoDetect.Image")));
            this.btDoDetect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btDoDetect.Name = "btDoDetect";
            this.btDoDetect.Size = new System.Drawing.Size(78, 22);
            this.btDoDetect.Text = "detect BPM";
            this.btDoDetect.Click += new System.EventHandler(this.btDoDetect_Click);
            // 
            // btListStop
            // 
            this.btListStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btListStop.Enabled = false;
            this.btListStop.Image = ((System.Drawing.Image)(resources.GetObject("btListStop.Image")));
            this.btListStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btListStop.Name = "btListStop";
            this.btListStop.Size = new System.Drawing.Size(37, 22);
            this.btListStop.Text = "stop";
            this.btListStop.Click += new System.EventHandler(this.btListStop_Click);
            // 
            // btWrite
            // 
            this.btWrite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btWrite.Image = ((System.Drawing.Image)(resources.GetObject("btWrite.Image")));
            this.btWrite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btWrite.Name = "btWrite";
            this.btWrite.Size = new System.Drawing.Size(71, 22);
            this.btWrite.Text = "WriteBpm";
            this.btWrite.Click += new System.EventHandler(this.btWrite_Click);
            // 
            // btGeneratePlaylist
            // 
            this.btGeneratePlaylist.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btGeneratePlaylist.Image = ((System.Drawing.Image)(resources.GetObject("btGeneratePlaylist.Image")));
            this.btGeneratePlaylist.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btGeneratePlaylist.Name = "btGeneratePlaylist";
            this.btGeneratePlaylist.Size = new System.Drawing.Size(109, 22);
            this.btGeneratePlaylist.Text = "generate playlist";
            this.btGeneratePlaylist.Click += new System.EventHandler(this.btGeneratePlayList_Click);
            // 
            // btWritePlaylistToiTunes
            // 
            this.btWritePlaylistToiTunes.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btWritePlaylistToiTunes.Image = ((System.Drawing.Image)(resources.GetObject("btWritePlaylistToiTunes.Image")));
            this.btWritePlaylistToiTunes.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btWritePlaylistToiTunes.Name = "btWritePlaylistToiTunes";
            this.btWritePlaylistToiTunes.Size = new System.Drawing.Size(87, 22);
            this.btWritePlaylistToiTunes.Text = "write playlist";
            this.btWritePlaylistToiTunes.Click += new System.EventHandler(this.btWritePlaylistToiTunes_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(793, 304);
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "Form1";
            this.Text = "BPM detect and Playlist generate";
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tbpBPMDetect.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.charBPM)).EndInit();
            this.tbpPlaylist.ResumeLayout(false);
            this.tbpPlaylist.PerformLayout();
            this.tbpConsole.ResumeLayout(false);
            this.tbpConsole.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.TextBox txConsole;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataVisualization.Charting.Chart charBPM;
        private System.Windows.Forms.ToolStripButton btOpen;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tbpConsole;
        private System.Windows.Forms.TabPage tbpBPMDetect;
        private System.Windows.Forms.ToolStripButton btiTunes;
        private System.Windows.Forms.ListView lviTuneTracks;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ToolStripButton btListStop;
        private System.Windows.Forms.ToolStripButton btDoDetect;
        private System.Windows.Forms.TreeView tvPlayList;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lbTitle;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripButton btWrite;
        private System.Windows.Forms.TextBox txBpmDetectSettings;
        private System.Windows.Forms.TabPage tbpPlaylist;
        private System.Windows.Forms.ListView lvPlayList;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.TextBox txPlaylistSettings;
        private System.Windows.Forms.ToolStripButton btGeneratePlaylist;
        private System.Windows.Forms.ToolStripButton btWritePlaylistToiTunes;
    }
}

