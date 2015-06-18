using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SoundAnalyzeLib;
using iTunesLib;

namespace BpmDetectorw
{
    public class TrackToBPMConverter : IValueConverter
    {
        public static Dictionary<int, BpmDetector> dictionary = new Dictionary<int, BpmDetector>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IITTrack track = value as IITTrack;
            if (track != null)
            {
                BpmDetector detector;
                if (dictionary.TryGetValue(track.trackID, out detector))
                {
                    return detector.BPM;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
