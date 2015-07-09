using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using iTunesLib;

namespace ComUtils
{
    public class BpmUtils
    {
        public const string STR_COMPILATION = "compilation";
        public static string cleanFileName(string value)
        {
            StringBuilder sb = new StringBuilder(value);
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                sb.Replace(c, '~');
            }
            return sb.ToString();
        }
        public static string getDataFileName(string root, IITTrack track, string ext)
        {
            string dir;
            string fileName;
            if (track.Compilation)
            {
                dir = Path.Combine(root, STR_COMPILATION, cleanFileName(track.Album));
                fileName = cleanFileName(string.Format("{0}_{1}_{2}", track.TrackNumber, track.Artist, track.Name));
            }
            else
            {
                dir = Path.Combine(root, cleanFileName(track.Artist), cleanFileName(track.Album));
                fileName = cleanFileName(string.Format("{0}_{1}", track.TrackNumber, track.Name));
            } if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.ChangeExtension(Path.Combine(dir, fileName), ext);
        }
    }
}
