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
    class TrackToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IITFileOrCDTrack track = value as IITFileOrCDTrack;
            if (track != null)
            {
                if (File.Exists(track.Location))
                {
                    string dir = Path.GetDirectoryName(track.Location);
                    string albumSmall = Path.Combine(dir, "AlbumArtSmall.jpg");
                    if (File.Exists(albumSmall))
                    {
                        return albumSmall;
                    }
                    String[] files = Directory.GetFiles(dir, "*.jpg");
                    if (files != null && files.Length > 0)
                    {
                        return files.First();
                    }
                }
                if (track.Artwork != null && track.Artwork.Count > 0)
                {

                    string dir = Path.Combine(Path.GetTempPath(), Properties.Resources.tempImageFolderName);
                    string fileBody;
                    if (string.IsNullOrEmpty(track.AlbumArtist) && string.IsNullOrEmpty(track.Album))
                    {
                        fileBody = track.Name;
                    }
                    else
                    {
                        fileBody = track.Artist + "_" + track.Album;
                    }
                    fileBody = Path.GetInvalidFileNameChars().Aggregate(fileBody, (current, c) => current.Replace(c.ToString(), string.Empty));
                    string fileName = Path.Combine(dir, fileBody);
                    switch (track.Artwork[1].Format)
                    {
                        case ITArtworkFormat.ITArtworkFormatBMP:
                            fileName = Path.ChangeExtension(fileName, "bmp");
                            break;
                        case ITArtworkFormat.ITArtworkFormatJPEG:
                            fileName = Path.ChangeExtension(fileName, "jpg");
                            break;
                        case ITArtworkFormat.ITArtworkFormatPNG:
                            fileName = Path.ChangeExtension(fileName, "png");
                            break;
                    }
                    if (!File.Exists(fileName))
                    {
                        track.Artwork[1].SaveArtworkToFile(fileName);
                    }
                    return fileName;
                }
            }

            return "pack://siteoforigin:,,,/Resources/m_e_others_501.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BPMToIntervalConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int bpm = (int)value;
            if (bpm == 0)
            {
                return 0.2;
            }
            else
            {
                return 60 / (double)bpm;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
