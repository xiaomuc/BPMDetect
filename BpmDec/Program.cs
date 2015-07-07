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
        bool _skip = false;
        string _dataPath;
        string _itunesLib = "";
        Dictionary<int, int> bpmResult;
        iTunesApp app;
        IITPlaylist playlist;

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
                Console.WriteLine("/S\t分析済みをスキップする");
                Console.WriteLine("/F\t音量測定のフレームサイズ。Defalut={0}", _config.FrameSize);
                Console.WriteLine("/C\t自己相関測定サイズ。Defalut={0}", _config.AutoCorrelationSize);
                Console.WriteLine("/B\tBPM検出範囲。Defalut={0}-{1}", _config.BPMLow, _config.BPMHigh);
                Console.WriteLine("/P\t優先するBPM範囲。Defalut={0}-{1}", _config.PriorityBPMLow, _config.PriorityBPMHigh);
                Console.WriteLine("/T\tピーク検出する閾値。Defalut={0}", _config.PeakThreshold);
                return -1;
            }
            foreach (string arg in args)
            {
                if (arg.StartsWith("/i") || arg.StartsWith("/I"))
                {
                    string[] param = arg.Split('/');
                    _itunesLib = param[1];

                }
                else if (arg.StartsWith("/d") || arg.StartsWith("/D"))
                {
                    string[] param = arg.Split('/');
                    _dataPath = param[1];
                }
                else if (arg.StartsWith("/f") || arg.StartsWith("/F"))
                {
                    string[] param = arg.Split('/');
                    int i;
                    if (int.TryParse(param[1], out i))
                    {
                        _config.FrameSize = i;
                    }
                }
                else if (arg.StartsWith("/c") || arg.StartsWith("/C"))
                {
                    string[] param = arg.Split('/');
                    int i;
                    if (int.TryParse(param[1], out i))
                    {
                        _config.AutoCorrelationSize = i;
                    }
                }
                else if (arg.StartsWith("/b") || arg.StartsWith("/B"))
                {
                    string[] param = arg.Split('/', '-');
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
                    string[] param = arg.Split('/', '-');
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
                    string[] param = arg.Split('/');
                    double d;
                    if (double.TryParse(param[1], out d))
                    {
                        _config.PeakThreshold = d;
                    }
                }
                else if (arg.StartsWith("/S") || arg.StartsWith("/s"))
                {
                    _skip = true;
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
        string getDataPath(IITTrack track)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(track.Artist + "_" + track.Album + "_" + track.Name + ".dat");
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                sb.Replace(c, '~');
            }
            return System.IO.Path.Combine(_dataPath, sb.ToString());
        }
        int doDetetection()
        {
            bpmResult = new Dictionary<int, int>();
            app = new iTunesApp();
            playlist = app.LibraryPlaylist;
            foreach (IITPlaylist pl in app.LibrarySource.Playlists)
            {
                if (pl.Name.Equals(_itunesLib))
                {
                    playlist = pl;
                    break;
                }
            }
            int counter = 0;
            foreach (IITFileOrCDTrack track in playlist.Tracks)
            {
                counter++;
                Console.WriteLine("[{0}/{1}]", counter, playlist.Tracks.Count);
                Console.WriteLine(" Title :{0}", track.Name);
                Console.WriteLine(" Album :{0}", track.Album);
                Console.WriteLine(" Artist:{0}", track.Artist);
                int prevBPM = track.BPM;
                string fileName = getDataPath(track);
                if (_skip && File.Exists(fileName))
                {
                    Console.WriteLine(" BPM   :{0} -> SKIP", prevBPM);
                }
                else
                {
                    try
                    {
                        BPMVolumeAutoCorrelation detector = new BPMVolumeAutoCorrelation(_config);
                        int bpm = detector.detect(track.Location);
                        detector.saveToFile(fileName);

                        bpmResult.Add(track.TrackDatabaseID, bpm);
                        Console.WriteLine(" BPM   :{0} -> {1}", prevBPM, bpm);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" BPM   :{0} -> ERROR", prevBPM);
                        Console.WriteLine(ex.Message);
                    }
                }
                Console.WriteLine("");
            }
            return counter;
        }
        void writeiTunes()
        {
            foreach (IITTrack track in playlist.Tracks)
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

        static int Main(string[] args)
        {
            instance = new Program();
            int ret = instance.getArgs(args);
            if (ret < 0) return ret;
            ret = instance.doDetetection();
            System.Threading.Thread.Sleep(1500);
            instance.writeiTunes();
            return ret;
        }
    }
}
