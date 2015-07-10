using System;
using System.Windows.Data;

namespace BpmDetector.TreeList
{
    /// <summary>
    /// Chart表示用にBPM値（1分間の拍数）から1拍の時間（秒）に変換する
    /// </summary>
    public class BPMToIntervalConverter : IValueConverter
    {
        /// <summary>
        /// BPM->秒変換
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 逆向き変換は使用しない
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
