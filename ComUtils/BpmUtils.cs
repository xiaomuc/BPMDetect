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
            string dir = Path.Combine(root, cleanFileName(track.Artist), cleanFileName(track.Album));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return Path.ChangeExtension(Path.Combine(dir, cleanFileName(track.Name)), ext);
        }
    }
}
