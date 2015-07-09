using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iTunesLib;
using SoundAnalyzeLib;

namespace BpmDec
{
    class Program
    {
        enum ExecMode { detect, clear, initbpm, help };
        ExecMode _mode = ExecMode.detect;
        static Program instance = null;
        BPMDetectorConfig _config;
        bool _forceAnalyze = false;
        bool _overwrite = true;
        string _dataPath;
        string _ext = "dat";
        string _itunesLib = "";
        iTunesApp _app;
        IITPlaylist _playlist;

        /// <summary>
        /// iTunes Appのインスタンス取得
        /// </summary>
        /// <returns></returns>
        iTunesApp getiTunes()
        {
            if (_app == null)
            {
                _app = new iTunesApp();
            }
            return _app;
        }

        /// <summary>
        /// ヘルプ表示
        /// </summary>
        void showHelp()
        {
            Console.WriteLine("usage:");
            Console.Write(AppDomain.CurrentDomain.SetupInformation.ApplicationName);
            Console.WriteLine("[command][/A][/N][/I:playlist_Name][/D:save_dir_name][/F:99][/C:99][/B:99-99][/P:99-99][/T:0.9]");
            Console.WriteLine("");
            Console.WriteLine("オプションの説明。[]は省略時のデフォルト値");
            Console.WriteLine("command:[detect]:BPM検出、");
            Console.WriteLine("\tclear\t:分析結果ファイルの消去、");
            Console.WriteLine("\tinitbpm\t:iTunesに設定されているBPM値を削除する、");
            Console.WriteLine("\thelp\t:これ。");
            Console.WriteLine("/A\t:分析済みでも再分析する");
            Console.WriteLine("/N\t:BPMをiTunesに書き込まない");
            Console.WriteLine("/I\t:対象iTunesプレイリスト\t[libray]");
            Console.WriteLine("/D\t:分析結果の保存先\t[{0}]", _dataPath);
            Console.WriteLine("/F\t:音量測定フレームサイズ\t[{0}]", _config.FrameSize);
            Console.WriteLine("/C\t:自己相関計測フレーム数\t[{0}]", _config.AutoCorrelationSize);
            Console.WriteLine("/B\t:BPMを検出する範囲\t[{0}-{1}]", _config.BPMLow, _config.BPMHigh);
            Console.WriteLine("/P\t:優先するBPMの範囲\t[{0}-{1}]", _config.PriorityBPMLow, _config.PriorityBPMHigh);
            Console.WriteLine("/T\t:ピーク検出するしきい値\t[{0}]", _config.PeakThreshold);
        }

        /// <summary>
        /// 引数の判定処理
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        ExecMode getArgs(string[] args)
        {
            _config = new BPMDetectorConfig();
            _dataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "BpmDetecor");
            
            foreach (string arg in args)
            {
                if (arg.StartsWith(@"/?") || arg.StartsWith("/h") 
                    || arg.StartsWith("/H") || arg.ToLower().Equals("help"))
                {
                    _mode = ExecMode.help;
                } else if (arg.StartsWith("/i") || arg.StartsWith("/I"))
                {
                    string[] param = arg.Split(':');
                    _itunesLib = param[1];

                }
                else if (arg.StartsWith("/d") || arg.StartsWith("/D"))
                {
                    string[] param = arg.Split('/');
                    _dataPath = param[1];
                }
                else if (arg.StartsWith("/f") || arg.StartsWith("/F"))
                {
                    string[] param = arg.Split(':');
                    int i;
                    if (int.TryParse(param[1], out i))
                    {
                        _config.FrameSize = i;
                    }
                }
                else if (arg.StartsWith("/c") || arg.StartsWith("/C"))
                {
                    string[] param = arg.Split(':');
                    int i;
                    if (int.TryParse(param[1], out i))
                    {
                        _config.AutoCorrelationSize = i;
                    }
                }
                else if (arg.StartsWith("/b") || arg.StartsWith("/B"))
                {
                    string[] param = arg.Split(':', '-');
                    int i;
                    if (int.TryParse(param[1], out i))
                    {
                        _config.BPMLow = i;
                    }
                    if (int.TryParse(param[2], out i))
                    {
                        _config.BPMHigh = i;
                    }
                }
                else if (arg.StartsWith("/p") || arg.StartsWith("/P"))
                {
                    string[] param = arg.Split(':', '-');
                    int i;
                    if (int.TryParse(param[1], out i))
                    {
                        _config.PriorityBPMLow = i;
                    }
                    if (int.TryParse(param[2], out i))
                    {
                        _config.PriorityBPMHigh = i;
                    }
                }
                else if (arg.StartsWith("/c") || arg.StartsWith("/C"))
                {
                    string[] param = arg.Split(':');
                    double d;
                    if (double.TryParse(param[1], out d))
                    {
                        _config.PeakThreshold = d;
                    }
                }
                else if (arg.StartsWith("/A") || arg.StartsWith("/a"))
                {
                    _forceAnalyze = true;
                }
                else if (arg.StartsWith("/N") || arg.StartsWith("/n"))
                {
                    _overwrite = false;
                }
                else if (arg.ToLower().Equals("clear"))
                {
                    _mode = ExecMode.clear;
                }
                else if (arg.ToLower().Equals("initbpm"))
                {
                    _mode = ExecMode.initbpm;
                }
            }
            return _mode;
        }

        /// <summary>
        /// プレイリストを探す。見つからない場合は既定のLibraryを返す
        /// </summary>
        /// <param name="name"></param>
        void findPlaylist(string name)
        {
            iTunesApp app = getiTunes();
            _playlist = app.LibraryPlaylist;
            foreach (IITPlaylist pl in app.LibrarySource.Playlists)
            {
                if (pl.Name.Equals(_itunesLib))
                {
                    _playlist = pl;
                    break;
                }
            }
        }

        /// <summary>
        /// iTunes経由で対象プレイリストにある曲のBPMをクリアする
        /// </summary>
        void BPMClear()
        {
            findPlaylist(_itunesLib);
            Console.WriteLine("Delete BPM for itunes::{0}", _playlist.Name);
            foreach (IITTrack track in _playlist.Tracks)
            {
                try
                {
                    track.BPM = 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:{0}/{1}/{2}", track.Name, track.Album, track.Artist);
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// BPM検出してiTunes経由で書き込む
        /// </summary>
        /// <returns></returns>
        int doDetetection()
        {
            findPlaylist(_itunesLib);
            if (_forceAnalyze)
            {
                Console.WriteLine("/A:分析済みでも再分析");
            }
            if (!_overwrite)
            {
                Console.WriteLine("/N:BPMを書き込まない");
            }
            Console.WriteLine("プレイリスト\t:{0}", _playlist.Name);
            Console.WriteLine("分析結果\t:{0}", _dataPath);
            Console.WriteLine("フレーム\t:{0}", _config.FrameSize);
            Console.WriteLine("自己相関測定\t:{0}", _config.AutoCorrelationSize);
            Console.WriteLine("BPM検出範囲\t:{0}-{1}", _config.BPMLow, _config.BPMHigh);
            Console.WriteLine("優先BPM範囲\t:{0}-{1}", _config.PriorityBPMLow, _config.PriorityBPMHigh);
            Console.WriteLine("ピーク検出\t:{0}", _config.PeakThreshold);

            int counter = 0;
            foreach (IITFileOrCDTrack track in _playlist.Tracks)
            {
                counter++;
                Console.Write("[{0}/{1}]", counter, _playlist.Tracks.Count);
                Console.WriteLine("{0}/{1}/{2}/[{3}]", track.Name,track.Album,track.Artist,track.Time);
                int bpm = track.BPM; ;
                string fileName = ComUtils.BpmUtils.getDataFileName(_dataPath, track, _ext);
                if (_forceAnalyze || !File.Exists(fileName))
                {
                    try
                    {
                        BPMVolumeAutoCorrelation detector = new BPMVolumeAutoCorrelation(_config);
                        int prev = bpm;
                        bpm = detector.detect(track.Location);
                        detector.saveToFile(fileName);
                        Console.WriteLine("\tBPM:{0} -> {1}", prev, bpm);
                        if (bpm != prev)
                        {
                            track.BPM = bpm;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else if (_overwrite)
                {
                    try
                    {
                        BPMVolumeAutoCorrelation detector = new BPMVolumeAutoCorrelation();
                        detector.loadFromFile(fileName);
                        bpm = detector.BPM;
                        Console.WriteLine("\tBPM:{0} -> {1}", track.BPM, bpm);
                        if (track.BPM != bpm)
                        {
                            track.BPM = bpm;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return counter;
        }

        /// <summary>
        /// 分析結果フォルダをフォルダごと削除
        /// </summary>
        void doClear()
        {
            Console.WriteLine("clear:{0}", _dataPath);
            if (Directory.Exists(_dataPath))
            {
                Directory.Delete(_dataPath, true);
            }
            else
            {
                Console.WriteLine("..not exists.");
            }
        }

        /// <summary>
        /// メイン処理部。モード判定とオプションにより動作を決定する
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("BPM detector for iTunes ver 0.1 by xiaomu 2015.");
            instance = new Program();
            ExecMode mode = instance.getArgs(args);
            switch (mode)
            {
                case ExecMode.help:
                    instance.showHelp();
                    break;
                case ExecMode.clear:
                    instance.doClear();
                    break;
                case ExecMode.initbpm:
                    instance.BPMClear();
                    break;
                case ExecMode.detect:
                    instance.findPlaylist(instance._itunesLib);
                    instance.doDetetection();
                    break;
            }
            Console.WriteLine("press any key to exit.");
            Console.ReadLine();
        }
    }
}
