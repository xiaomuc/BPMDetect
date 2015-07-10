using System;
using System.Linq;
using System.Windows.Data;
using System.IO;
using iTunesLib;

namespace BpmDetector.TreeList
{
    class TrackToImageSourceConverter : IValueConverter
    {
        public static string _imagePath = string.Empty;
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
                    if (string.IsNullOrEmpty(_imagePath))
                    {
                        _imagePath = Path.Combine(Path.GetTempPath(), "bpmDetector");
                    }
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
                    string fileName = Path.Combine(_imagePath, fileBody);
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

            //return "pack://siteoforigin:,,,/Resources/m_e_others_501.png";
            return "image/m_e_others_501.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



}
