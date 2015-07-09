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
        BackgroundWorker _bgwDetection;
        BackgroundWorker _bgwWriteiTune;
        BitmapImage biPause;
        BitmapImage biPlay;
        DateTime _startTime;
        string _dataPath;
        string _ext = "dat";
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
            _itunesApp.OnPlayerPlayEvent += _itunesApp_OnPlayerPlayEvent;
            _itunesApp.OnPlayerStopEvent += _itunesApp_OnPlayerStopEvent;

            //BPM検出オブジェクトを格納するリスト(ディクショナリ)
            _detectorDictionary = new Dictionary<int, IBpmDetector>();

            //BPM検出結果を保存するフォルダ
            _dataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "BpmDetecor");
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }

            //ツリー表示用にプレイリストを保持するオブジェクト
            PlaylistTreeItem.createPlaylistTree(trvPlayList, _itunesApp.LibrarySource, _detectorDictionary, _dataPath, _ext);

            //アルバムアートワーク用の設定
            _imagePath = System.IO.Path.Combine(_dataPath, Properties.Resources.tempImageFolderName);
            deleteImageDir();
            TrackToImageSourceConverter._imagePath = _imagePath;
            Directory.CreateDirectory(_imagePath);

            //BPMバックグラウンドスレッドの準備
            _bgwDetection = new BackgroundWorker();
            _bgwDetection.WorkerReportsProgress = true;
            _bgwDetection.WorkerSupportsCancellation = true;
            _bgwDetection.DoWork += _bgwDetection_DoWork;
            _bgwDetection.ProgressChanged += _bgwDetection_ProgressChanged;
            _bgwDetection.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;

            //iTunesイベント管理(未完成)
            biPlay = new BitmapImage(new Uri("image/1435735759_play.png", UriKind.Relative));
            biPause = new BitmapImage(new Uri("image/1435735785_pause.png", UriKind.Relative));

            //iTunes書き込みバッググランド処理の準備
            _bgwWriteiTune = new BackgroundWorker();
            _bgwWriteiTune.WorkerReportsProgress = true;
            _bgwWriteiTune.WorkerSupportsCancellation = true;
            _bgwWriteiTune.DoWork += _bgwWriteiTune_DoWork;
            _bgwWriteiTune.ProgressChanged += _bgwWriteiTune_ProgressChanged;
            _bgwWriteiTune.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
        }

        /// <summary>
        /// ウインドウが閉じる際にアルバムアートワーク用のテンポラリフォルダを削除しとく
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            _itunesApp.OnPlayerPlayEvent -= _itunesApp_OnPlayerPlayEvent;
            _itunesApp.OnPlayerStopEvent -= _itunesApp_OnPlayerStopEvent;
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

        /// <summary>
        /// iTunes経由でBPM情報を書き込む
        /// </summary>
        void doWriteiTunes()
        {
            if (_bgwWriteiTune.IsBusy)
            {
                btnWriteToiTunes.IsEnabled = false;
                _bgwWriteiTune.CancelAsync();
            }
            else
            {
                btnWriteToiTunes.Content = "Cancel";
                PlaylistTreeItem item = trvPlayList.SelectedItem as PlaylistTreeItem;
                prbDetecting.Value = 0;
                sbiProgress.Visibility = Visibility.Visible;
                sbiDetectorStatus.Content = "Writing to iTunes..";
                _bgwWriteiTune.RunWorkerAsync(item.Tracks);
            }
        }

        /// <summary>
        /// 引数trackを内包しているTrackWrapperオブジェクトを検索
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        TrackWrapper getTrackWrapper(IITTrack track)
        {
            TrackCollectionWrapper collection = lvTracks.ItemsSource as TrackCollectionWrapper;
            if (collection != null)
            {
                return collection.get(track);
            }
            return null;
        }

        /// <summary>
        /// 引数のオブジェクトで指定された項目をリスト上で選択、表示する
        /// </summary>
        /// <param name="tw"></param>
        void showTrack(TrackWrapper tw)
        {
            lvTracks.SelectedItem = tw;
            lvTracks.ScrollIntoView(tw);
        }

        /// <summary>
        /// 引数のオブジェクトで指定された項目をリスト上で選択、表示する
        /// </summary>
        /// <param name="track"></param>
        void showTrack(IITTrack track)
        {
            TrackWrapper tw = getTrackWrapper(track);
            showTrack(tw);
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
            if (!_bgwDetection.IsBusy)
            {
                _startTime = DateTime.Now;
                bool bOverwrite = cmbDetectedTrack.SelectedIndex == 1;
                WriteMode mode;
                switch (cmbWriteMode.SelectedIndex)
                {
                    case 1: mode = WriteMode.Immediate;
                        break;
                    case 2: mode = WriteMode.AfterAll;
                        break;
                    default: mode = WriteMode.Manual;
                        break;
                }
                btnDetectAll.Content = "Stop";
                btnWriteToiTunes.IsEnabled = false;
                PlaylistTreeItem item = trvPlayList.SelectedItem as PlaylistTreeItem;
                BackgroundArgument ba = new BackgroundArgument()
                {
                    Config = createConfig(),
                    Tracks = item.Tracks,
                    Overwrite = bOverwrite,
                    WriteToiTunes = mode
                };
                prbDetecting.Value = 0;
                sbiProgress.Visibility = Visibility.Visible;
                _bgwDetection.RunWorkerAsync(ba);
            }
            //実行中なら止める
            else
            {
                btnDetectAll.IsEnabled = false;
                _bgwDetection.CancelAsync();
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
            if (_itunesApp.PlayerState == ITPlayerState.ITPlayerStateStopped)
            {
                TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
                if (tw != null)
                {
                    tw.Track.Play();
                }
            }
            else if (_itunesApp.PlayerState == ITPlayerState.ITPlayerStatePlaying)
            {
                _itunesApp.Stop();
            }
        }

        /// <summary>
        /// 停止ボタン処理。iTunesでの再生を停止する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        private void btnWriteToiTunes_Click(object sender, RoutedEventArgs e)
        {
            doWriteiTunes();
        }

        /// <summary>
        /// 進むボタン処理。リスト上の一つ後を選択、再生中ならそいつを再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            if (lvTracks.SelectedIndex == -1)
            {
                lvTracks.SelectedIndex = 0;
            }
            else
            {
                lvTracks.SelectedIndex++;
            }
            lvTracks.ScrollIntoView(lvTracks.SelectedItem);
            if (_itunesApp.PlayerState == ITPlayerState.ITPlayerStatePlaying)
            {
                TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
                tw.Track.Play();
            }
        }

        /// <summary>
        /// 戻るボタン処理。リスト上の一つ前を選択、再生中ならそいつを再生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPrev_Click(object sender, RoutedEventArgs e)
        {
            if (lvTracks.SelectedIndex <= 0)
            {
                lvTracks.SelectedIndex = 0;
            }
            lvTracks.Items.MoveCurrentToPrevious();
            lvTracks.ScrollIntoView(lvTracks.SelectedItem);
            if (_itunesApp.PlayerState == ITPlayerState.ITPlayerStatePlaying)
            {
                TrackWrapper tw = lvTracks.SelectedItem as TrackWrapper;
                tw.Track.Play();
            }
        }

        #endregion

        #region Background Worker

        /// <summary>
        /// バックグラウンド処理完了通知ハンドラ。BPM特定とiTunes書き込みの共通処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            btnDetectAll.Content = "Start";
            btnDetectAll.IsEnabled = true;

            btnWriteToiTunes.Content = "Write to iTunes";
            btnWriteToiTunes.IsEnabled = true;

            sbiDetectorStatus.Content = string.Empty;
            sbiProgress.Visibility = Visibility.Hidden;

            sbiDuration.Content = string.Empty;
            if (!e.Cancelled && e.Result != null)
            {
                BackgroundArgument ba = e.Result as BackgroundArgument;
                if (ba.WriteToiTunes == WriteMode.AfterAll)
                {
                    doWriteiTunes();
                }
            }
        }

        /// <summary>
        /// バックグラウンド処理の途中経過ハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _bgwDetection_ProgressChanged(object sender, ProgressChangedEventArgs e)
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

                TrackCollectionWrapper collection = lvTracks.ItemsSource as TrackCollectionWrapper;
                TrackWrapper tw = collection.get(userState.Track);
                tw.Detector = userState.Detector;


                if (userState.WriteToiTunes == WriteMode.Immediate)
                {
                    try
                    {
                        tw.Track.BPM = userState.Detector.BPM;
                        tw.notifyPropertyChanged("Track");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                if (_itunesApp.PlayerState == ITPlayerState.ITPlayerStateStopped)
                {
                    showTrack(userState.Track);
                }
                TimeSpan ts = DateTime.Now - _startTime;
                sbiDuration.Content = ts.ToString(@"hh\:mm\:ss");
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
        void _bgwDetection_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            BackgroundArgument ba = e.Argument as BackgroundArgument;
            foreach (TrackWrapper tw in ba.Tracks)
            {
                if (_bgwDetection.CancellationPending)
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
                        userState.WriteToiTunes = ba.WriteToiTunes;
                        worker.ReportProgress(percent, userState);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            e.Result = ba;
        }

        void _bgwWriteiTune_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //プログレスバーを更新しておく
            prbDetecting.Value = e.ProgressPercentage;
            TrackWrapper tw = e.UserState as TrackWrapper;
            foreach (TrackWrapper t in lvTracks.Items)
            {
                if (t.Track.TrackDatabaseID == tw.Track.TrackDatabaseID)
                {
                    t.notifyPropertyChanged("Track");
                    break;
                }
            }
        }

        void _bgwWriteiTune_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            TrackCollectionWrapper collection = e.Argument as TrackCollectionWrapper;
            foreach (TrackWrapper tw in collection)
            {
                if (worker.CancellationPending) break;
                int percent = (int)(100 * (double)tw.Index / (double)collection.Count);

                if (tw.Detector != null && tw.Detector.BPM > 0 && tw.Detector.BPM != tw.Track.BPM)
                {
                    try
                    {
                        tw.Track.BPM = tw.Detector.BPM;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                worker.ReportProgress(percent, tw);
            }
        }

        #endregion

        #region iTunes Events
        /// <summary>
        /// iTunesからの再生停止イベントハンドラ。うまく動いてない
        /// </summary>
        /// <param name="iTrack"></param>
        void _itunesApp_OnPlayerStopEvent(object iTrack)
        {
            imgPlayback.Dispatcher.BeginInvoke(new Action(() =>
            {
                imgPlayback.Source = biPlay;
            }));
        }

        /// <summary>
        /// iTunesからの再生開始イベントハンドラ
        /// </summary>
        /// <param name="iTrack"></param>
        private void _itunesApp_OnPlayerPlayEvent(object iTrack)
        {
            imgPlayback.Dispatcher.BeginInvoke(new Action(() =>
            {
                imgPlayback.Source = biPause;
            }));
            lvTracks.Dispatcher.BeginInvoke(
                     new Action(() =>
                     {
                         IITTrack track = iTrack as IITTrack;
                         showTrack(track);
                     }));
        }
        #endregion



    }

    enum WriteMode { Manual, Immediate, AfterAll };

    /// <summary>
    /// バックグラウンド処理用の引数クラス
    /// </summary>
    class BackgroundArgument
    {
        public BPMDetectorConfig Config { get; set; }
        public TrackCollectionWrapper Tracks { get; set; }
        public bool Overwrite { get; set; }
        public WriteMode WriteToiTunes { get; set; }
    }

    /// <summary>
    /// バックグラウンド処理の途中経過通知用クラス
    /// </summary>
    class BackgroundUserState
    {
        public IITTrack Track { get; set; }
        public IBpmDetector Detector { get; set; }
        public WriteMode WriteToiTunes { get; set; }
    }
}

