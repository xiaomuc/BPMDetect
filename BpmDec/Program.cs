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
        static Program instance = null;
        BPMDetectorConfig _config;
        bool _forceAnalyze = false;
        bool _overwrite = true;
        string _dataPath;
        string _ext = "dat";
        string _itunesLib = "";
        Dictionary<int, int> bpmResult;
        iTunesApp _app;
        IITPlaylist _playlist;
        bool _clear=false;

        iTunesApp getiTunes()
        {
            if (_app == null)
            {
                _app = new iTunesApp();
            }
            return _app;
        }

        int getArgs(string[] args)
        {
            _config = new BPMDetectorConfig();
            _dataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "BpmDetecor");
            if (args.Contains(@"/?") || args.Contains("/h") || args.Contains("/H"))
            {
                Console.Write("usage:");
                Console.Write(AppDomain.CurrentDomain.SetupInformation.ApplicationName);
                Console.WriteLine(" [/I:playlist_Name][/D:save_dir_name][/Skip][/F:99][/C:99][/B:99-99][/P:99-99][/T:0.9]");
                Console.WriteLine("/I\t分析対象のiTunesプレイリスト。Defalut=library");
                Console.WriteLine("/D\t分析結果保存ディレクトリ。Defalut={0}", _dataPath);
                Console.WriteLine("/A\t分析済みでも再分析する");
                Console.WriteLine("/N\t分析済みのBPMで上書きしない");
                Console.WriteLine("/F\t音量測定のフレームサイズ。Defalut={0}", _config.FrameSize);
                Console.WriteLine("/C\t自己相関測定サイズ。Defalut={0}", _config.AutoCorrelationSize);
                Console.WriteLine("/B\tBPM検出範囲。Defalut={0}-{1}", _config.BPMLow, _config.BPMHigh);
                Console.WriteLine("/P\t優先するBPM範囲。Defalut={0}-{1}", _config.PriorityBPMLow, _config.PriorityBPMHigh);
                Console.WriteLine("/T\tピーク検出する閾値。Defalut={0}", _config.PeakThreshold);
                Console.WriteLine("/L\tBPMをクリアする。Defalut={0}", _config.PeakThreshold);
                return -1;
            }
            foreach (string arg in args)
            {
                if (arg.StartsWith("/i") || arg.StartsWith("/I"))
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
                else if (arg.StartsWith("/L") || arg.StartsWith("/l"))
                {
                    _clear = true;
                }
            }
            Console.WriteLine("プレイリスト\t:{0}", _itunesLib);
            Console.WriteLine("分析結果\t:{0}", _dataPath);
            Console.WriteLine("フレーム\t:{0}", _config.FrameSize);
            Console.WriteLine("自己相関測定\t:{0}", _config.AutoCorrelationSize);
            Console.WriteLine("BPM検出範囲\t:{0}-{1}", _config.BPMLow, _config.BPMHigh);
            Console.WriteLine("優先BPM範囲\t:{0}-{1}", _config.PriorityBPMLow, _config.PriorityBPMHigh);
            Console.WriteLine("ピーク検出\t:{0}", _config.PeakThreshold);

            return 0;
        }
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
        void BPMClear()
        {
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
        int doDetetection()
        {
            if (_clear)
            {
                BPMClear();
                return 1;
            }
            bpmResult = new Dictionary<int, int>();
            int counter = 0;
            foreach (IITFileOrCDTrack track in _playlist.Tracks)
            {
                counter++;
                Console.WriteLine("[{0}/{1}]", counter, _playlist.Tracks.Count);
                Console.WriteLine(" Title :{0}", track.Name);
                Console.WriteLine(" Album :{0}", track.Album);
                Console.WriteLine(" Artist:{0}", track.Artist);
                int bpm = track.BPM; ;
                string fileName = ComUtils.BpmUtils.getDataFileName(_dataPath, track,_ext);
                if (_forceAnalyze || !File.Exists(fileName))
                {
                    try
                    {
                        BPMVolumeAutoCorrelation detector = new BPMVolumeAutoCorrelation(_config);
                        int prev = bpm;
                        bpm = detector.detect(track.Location);
                        detector.saveToFile(fileName);
                        Console.WriteLine(" BPM   :{0} -> {1}", prev, bpm);
                        if (bpm != prev)
                        {
                            track.BPM = bpm;
                        }
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        Console.WriteLine(ex.Message);
                        bpmResult.Add(track.TrackDatabaseID, bpm);

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
                        Console.WriteLine(" BPM   :{0} -> {1}", track.BPM, bpm);
                        if (track.BPM != bpm)
                        {
                            track.BPM = bpm;
                        }
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        Console.WriteLine(ex.Message);
                        bpmResult.Add(track.TrackDatabaseID, bpm);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                Console.WriteLine("");
            }
            return counter;
        }
        void writeiTunes()
        {
            if (_clear) return;

            foreach (IITTrack track in _playlist.Tracks)
            {
                if (bpmResult.ContainsKey(track.TrackDatabaseID))
                {
                    try
                    {
                        track.BPM = bpmResult[track.TrackDatabaseID];
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        Console.WriteLine(track.Name);
                        Console.WriteLine("result:{0:X}", ex.HResult);
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            instance = new Program();
            int ret = instance.getArgs(args);
            if (ret < 0) return;
            instance.findPlaylist(instance._itunesLib);
            ret = instance.doDetetection();
            System.Threading.Thread.Sleep(1500);
            instance.writeiTunes();
        }
    }
}
