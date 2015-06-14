using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.IO;
using iTunesLib;

namespace BpmDetectorw
{
    class TrackToImageSourceConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IITFileOrCDTrack track = value as IITFileOrCDTrack;
            if (track != null)
            {
                string albumSmall = Path.Combine(Path.GetDirectoryName(track.Location), "AlbumArtSmall.jpg");
                if (File.Exists(albumSmall))
                {
                    return albumSmall;
                }
            }

            return "pack://siteoforigin:,,,/Resources/noimage.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
