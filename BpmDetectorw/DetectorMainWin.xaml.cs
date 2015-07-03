using iTunesLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using SoundAnalyzeLib;

namespace BpmDetectorw
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DetectorMainWin : Window
    {
        #region Field
        iTunesApp _itunesApp;
        string _imagePath;
        Dictionary<int, IBpmDetector> _detectorDictionary;
        BackgroundWorker _backgroundWorker;
        BitmapImage biPause;
        BitmapImage biPlay;
        DriveObject _driveObject;
        #endregion

        #region Contructor/Destructor
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DetectorMainWin()
        {
            InitializeComponent();

            //iTunes COM Interface
            _itunesApp = new iTunesApp();

            //BPM検出オブジェクトを格納するリスト(ディクショナリ)
            _detectorDictionary = new Dictionary<int, IBpmDetector>();

            //ツリー表示用にプレイリストを保持するオブジェクト
            PlaylistTreeItem.createPlaylistTree(trvPlayList, _itunesApp.LibrarySource, _detectorDictionary);

            //アルバムアートワーク用の設定
            _imagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Properties.Resources.tempImageFolderName);
            deleteImageDir();
            Directory.CreateDirectory(_imagePath);

            //バックグラウンドスレッドの準備
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;

            //iTunesイベント管理(未完成)
            _driveObject = new DriveObject(imgPlayback, "1435735785_play.png", "1435735785_play.png");

            _itunesApp.OnPlayerPlayEvent += _itunesApp_OnPlayerPlayEvent;
            _itunesApp.OnPlayerStopEvent += _itunesApp_OnPlayerStopEvent;
        }

        /// <summary>
        /// ウインドウが閉じる際にアルバムアートワーク用のテンポラリフォルダを削除しとく
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            deleteImageDir();
        }
        #endregion

        #region Inner Method
        /// <summary>
        /// アルバムアートワーク用テンポラリフォルダの削除
        /// </summary>
        void deleteImageDir()
        {
            if (Directory.Exists(_imagePath))
            {
                try
                {
                    Directory.Delete(_imagePath, true);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    System.Console.Write(ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// BPM分析に使用する設定オブジェクトの生成
        /// </summary>
        /// <returns><seealso cref="BPMDetectorConfig"/></returns>
        BPMDetectorConfig createConfig()
        {
            return new BPMDetectorConfig()
            {
                BPMLow = (int)iupBPMLo.Value,
                BPMHigh = (int)iupBPMHi.Value,
                PriorityBPMLow = (int)iupPrioLo.Value,
                PriorityBPMHigh = (int)iupPrioHi.Value,
                PeakThreshold = (double)dupThreshold.Value,
                FrameSize = (int)iupFrameSize.Value,
                AutoCorrelationSize = (int)iupCorrelationSize.Value
            };
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// リストビューダブルクリック時に選択された曲のBPMを算出する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvTracks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item != null)
            {
                TrackWrapper tw = item.Content as TrackWrapper;
                IITFileOrCDTrack track = tw.Track as IITFileOrCDTrack;
                if (track != null)
                {
                    BPMDetectorConfig config = createConfig();

                    try
                    {
                        //IBpmDetector detector = new BpmDetector(config);
                        IBpmDetector detector = new BPMVolumeAutoCorrelation(config);
                        detector.detect(track.Location);
                        tw.Detector = detector;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 全トラックのBPM検出をバックグラウンドで実行する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDetectAll_Click(object sender, RoutedEventArgs e)
        {
            //実行中じゃなければ実行開始
            if (btnDetectAll.Content.Equals("Detect All"))
            {
                btnDetectAll.Content = "Stop";
                PlaylistTreeItem item = trvPlayList.SelectedItem as PlaylistTreeItem;
                BackgroundArgument ba = new BackgroundArgument()
                {
                    Config = createConfig(),
                    Tracks = item.Tracks,
                    Overwrite = (bool)chbOverwrite.IsChecked,
                    WriteToiTunesImmediate = false
                };
                prbDetecting.Value = 0;
                sbiProgress.Visibility = Visibility.Visible;
                _backgroundWorker.RunWorkerAsync(ba);
            }
            //実行中なら止める
            else
            {
                btnDetectAll.IsEnabled = false;
                _backgroundWorker.CancelAsync();
            }
        }

        /// <summary>
        /// タップ(クリック)でBPM検出したいけど、いまいちなの
        /// </summary>
        long prev_tick = 0;
        private void btnTap_Click(object sender, RoutedEventArgs e)
        {
            long now = DateTime.Now.Ticks;
            long tapped = now - prev_tick;
            prev_tick = now;
            if (tapped < TimeSpan.TicksPerMinute)
            {
                long bpm = TimeSpan.TicksPerMinute / tapped;
                tblTap.Text = bpm.ToString();
            }
        }

        /// <summary>
        /// iTunesで選択した曲を再生する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
            if (tw != null)
            {
                tw.Track.Play();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            _itunesApp.Stop();
        }

        /// <summary>
        /// 候補になっているBPMを選択するボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectBPM_Click(object sender, RoutedEventArgs e)
        {
            int bpm = (int)(sender as Button).Content;
            TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
            if (tw != null)
            {
                tw.Detector.BPM = bpm;
                tw.notifyPropertyChanged("Detector");
            }
        }

        /// <summary>
        /// 検出したBPMをiTunesに書き込む処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btWriteToiTunes_Click(object sender, RoutedEventArgs e)
        {
            foreach (TrackWrapper tw in lvTracks.Items)
            {
                try
                {
                    if (tw.Detector != null && tw.Detector.BPM > 0)
                    {
                        tw.Track.BPM = tw.Detector.BPM;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        #endregion

        #region Background Worker

        /// <summary>
        /// バックグラウンド処理完了通知ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnDetectAll.Content = "Detect All";
            btnDetectAll.IsEnabled = true;
            sbiDetectorStatus.Content = string.Empty;
            sbiProgress.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// バックグラウンド処理の途中経過ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //プログレスバーを更新しておく
            prbDetecting.Value = e.ProgressPercentage;

            //処理前の通知なので、曲名とかをステータスバーに表示
            if (e.UserState is IITTrack)
            {
                IITTrack track = e.UserState as IITTrack;
                sbiDetectorStatus.Content = string.Format("{0}:{1} by {2}", track.Index, track.Name, track.Artist);
            }
            //BPM検出完了の通知なので、リストに表示とかを行う
            else if (e.UserState is BackgroundUserState)
            {
                BackgroundUserState userState = e.UserState as BackgroundUserState;
                if (_detectorDictionary.ContainsKey(userState.Track.TrackDatabaseID))
                {
                    _detectorDictionary[userState.Track.TrackDatabaseID] = userState.Detector;
                }
                else
                {
                    _detectorDictionary.Add(userState.Track.TrackDatabaseID, userState.Detector);
                }

                foreach (TrackWrapper tw in lvTracks.Items)
                {
                    if (tw.Track.TrackDatabaseID.Equals(userState.Track.TrackDatabaseID))
                    {
                        tw.Detector = userState.Detector;
                    }
                }
            }
        }

        /// <summary>
        /// バックグラウンド処理の実装部
        /// </summary>
        /// <remarks>
        /// １曲ずつ処理してBPM検出し、通知を投げる
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            BackgroundArgument ba = e.Argument as BackgroundArgument;
            foreach (TrackWrapper tw in ba.Tracks)
            {
                if (_backgroundWorker.CancellationPending)
                {
                    break;
                }
                if (!ba.Overwrite && tw.Detector != null)
                {
                    continue;
                }
                int percent = (int)(100 * (double)tw.Index / (double)ba.Tracks.Count);
                IITFileOrCDTrack track = tw.Track as IITFileOrCDTrack;

                if (track != null)
                {
                    worker.ReportProgress(percent, track);
                    BackgroundUserState userState = new BackgroundUserState();
                    userState.Track = tw.Track;
                    try
                    {
                        userState.Detector = new BPMVolumeAutoCorrelation(ba.Config);
                        userState.Detector.detect(track.Location);
                        worker.ReportProgress(percent, userState);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        #endregion

        #region iTunes Events
        /// <summary>
        /// iTunesからの再生停止イベントハンドラ。うまく動いてない
        /// </summary>
        /// <param name="iTrack"></param>
        async void _itunesApp_OnPlayerStopEvent(object iTrack)
        {
            await System.Threading.Tasks.Task.Run(async () =>
            {
                if (!_driveObject.CheckAccess())
                {
                    await _driveObject.Dispatcher.InvokeAsync(() => _driveObject.setPlayImage());
                }
            });
        }

        /// <summary>
        /// iTunesからの再生開始イベントハンドラ
        /// </summary>
        /// <param name="iTrack"></param>
        private async void _itunesApp_OnPlayerPlayEvent(object iTrack)
        {
            await System.Threading.Tasks.Task.Run(async () =>
            {
                if (!_driveObject.CheckAccess())
                {
                    await _driveObject.Dispatcher.InvokeAsync(() => _driveObject.setPauseImage());
                }
            });
            //IITTrack track = iTrack as IITTrack;
            //foreach (TrackWrapper tw in lvTracks.Items)
            //{
            //    if (tw.Track.Equals(iTrack))
            //    {
            //        lvTracks.SelectedItem = tw;
            //        lvTracks.ScrollIntoView(tw);
            //    }
            //}
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    class DriveObject : DispatcherObject
    {
        BitmapImage _play;
        BitmapImage _pause;
        Image _img;
        public DriveObject(Image img, string playImage, string pauseImage)
        {
            _img = img;
            _play = new BitmapImage(new Uri(playImage, UriKind.Relative));
            _pause = new BitmapImage(new Uri(pauseImage, UriKind.Relative));
        }
        public void setPlayImage()
        {
            _img.Source = _play;
        }
        public void setPauseImage()
        {
            _img.Source = _pause;
        }

    }

    /// <summary>
    /// バックグラウンド処理用の引数クラス
    /// </summary>
    class BackgroundArgument
    {
        public BPMDetectorConfig Config { get; set; }
        public TrackCollectionWrapper Tracks { get; set; }
        public bool Overwrite { get; set; }
        public bool WriteToiTunesImmediate { get; set; }
    }

    /// <summary>
    /// バックグラウンド処理の途中経過通知用クラス
    /// </summary>
    class BackgroundUserState
    {
        public IITTrack Track { get; set; }
        public IBpmDetector Detector { get; set; }
    }
}

