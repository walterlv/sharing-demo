using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Walterlv.Demo.Media
{
    /// <summary>
    /// 包含将颜色值按照亮度进行转换的方法。
    /// </summary>
    public class ColorLuminanceConverter : IValueConverter
    {
        /// <summary>
        /// 默认的转换方法，等同于 <see cref="ReverseBackgroundToWhiteBlackForeground"/>。
        /// </summary>
        public static ColorLuminanceConverter Default { get; } = new ColorLuminanceConverter();

        /// <summary>
        /// 按背景色的亮度转换背景色，以得到一个能够在此背景色上清晰显示的前景色。
        /// </summary>
        public static ColorLuminanceConverter ReverseBackgroundToWhiteBlackForeground { get; } =
            new ColorLuminanceConverter
            {
                LuminanceToColor = luminance => luminance > 0.5 ? Colors.Black : Colors.White,
            };

        /// <summary>
        /// 获取或设置亮度到颜色的转换方法。
        /// </summary>
        public Func<double, Color> LuminanceToColor { get; set; }

        /// <summary>
        /// 尝试使用此 <see cref="ColorLuminanceConverter"/> 转换一个颜色。
        /// </summary>
        /// <param name="color">待转换的颜色。</param>
        /// <returns>转换后的颜色。</returns>
        public Color Convert(Color color)
        {
            var luminance = GetGrayLevel(color);
            return (LuminanceToColor ?? ReverseBackgroundToWhiteBlackForeground.LuminanceToColor)(luminance);
        }

        /// <summary>
        /// 获取一个颜色的人眼感知亮度，并以 0~1 之间的小数表示。
        /// </summary>
        private static double GetGrayLevel(Color color)
        {
            return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return Convert((Color) value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
