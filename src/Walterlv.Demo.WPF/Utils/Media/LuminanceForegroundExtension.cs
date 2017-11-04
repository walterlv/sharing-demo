using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Walterlv.Demo.Media
{
    /// <summary>
    /// 按背景色的亮度取能够在此背景色上清晰显示的前景色。
    /// </summary>
    public class LuminanceForegroundExtension : DependencyMarkupExtension
    {
        public string BackgroundTargetName { get; set; }

        public ColorLuminanceConverter Converter { get; set; }

        public Color Background { get; set; }

        protected override object ProvideValue(FrameworkElement target, DependencyProperty property)
        {
            if (CreatorDictionary.TryGetValue(property.PropertyType, out var create))
            {
                var color = FindSourceColor(target);
                var reversedColor = ReverseBackgroundColor(color);
                return create(reversedColor);
            }
            return property.DefaultMetadata.DefaultValue;
        }

        /// <summary>
        /// 找到此前景色获取所需的背景色。
        /// </summary>
        /// <returns>找到的背景色</returns>
        private Color FindSourceColor(FrameworkElement origin)
        {
//            var @object = origin.FindName("BackgroundTargetName") as DependencyObject;
//            @object.GetValue(Panel.BackgroundProperty);
            return Background;
        }

        /// <summary>
        /// 将背景色反为前景色。
        /// </summary>
        /// <param name="color">背景色</param>
        /// <returns>前景色</returns>
        private Color ReverseBackgroundColor(Color color)
        {
            return (Converter ?? ColorLuminanceConverter.Default).Convert(color);
        }

        /// <summary>
        /// 储存能够储存颜色的依赖项属性的类型，以及此属性取得某种颜色的方法。
        /// </summary>
        private static readonly Dictionary<Type, Func<Color, object>> CreatorDictionary =
            new Dictionary<Type, Func<Color, object>>
            {
                {typeof(Color), color => color},
                {
                    typeof(Brush), color =>
                    {
                        var brush = new SolidColorBrush(color);
                        brush.Freeze();
                        return brush;
                    }
                }
            };
    }
}
