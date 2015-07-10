using System.Text;
using System.IO;
using iTunesLib;

namespace ComUtils
{
    /// <summary>
    /// wpfとコマンドラインで共通する処理なんだけどiTunesを使うのでSoundLibには入れたくなかった。
    /// 分析結果を保存しておく場所とファイル名生成するだけなんだけどね。
    /// </summary>
    /// <remarks>
    /// 通常は<br/>
    /// [root]\[Artist]\[Album]\[TrackNumber]_[Name].dat
    /// コンピは<br/>
    /// [root]\complation\[TrackNumber]_[Artist]_[Name].dat
    /// </remarks>
    public class BpmUtils
    {
        /// <summary>
        /// コンピのアルバムはコンピに集める
        /// </summary>
        public const string STR_COMPILATION = "compilation";

        /// <summary>
        /// ファイル名に使えない文字を~に変換
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string cleanFileName(string value)
        {
            StringBuilder sb = new StringBuilder(value);
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                sb.Replace(c, '~');
            }
            return sb.ToString();
        }

        /// <summary>
        /// トラックからデータファイル保存パスを生成
        /// </summary>
        /// <param name="root"></param>
        /// <param name="track"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
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
