using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Walterlv.Demo.Converters;
using Walterlv.Demo.Xaml;

namespace Walterlv.Demo.Media
{
    /// <summary>
    /// 按背景色的亮度取能够在此背景色上清晰显示的前景色。
    /// </summary>
    public class LuminanceForegroundExtension : DependencyMarkupExtension
    {
        public string TargetName { get; set; }

        public ColorLuminanceConverter Converter { get; set; }

        protected override object ProvideValue(FrameworkElement target, DependencyProperty property)
        {
            if (CreatorDictionary.TryGetValue(property.PropertyType, out var create))
            {
                var (source, path) = FindSource(target);
                if (source != null && path != null)
                {
                    return new Binding(path)
                    {
                        Source = source,
                        Mode = BindingMode.OneWay,
                        Converter = new FuncConverter
                        {
                            Convert = sourceValue =>
                                create(ReverseBackgroundColor(((SolidColorBrush) sourceValue).Color)),
                        },
                    };
                }
            }
            return property.DefaultMetadata.DefaultValue;
        }

        /// <summary>
        /// 找到此前景色获取所需的背景色。
        /// </summary>
        /// <returns>找到的背景色</returns>
        private (object source, string property) FindSource(FrameworkElement origin)
        {
            if (!(origin.FindName(TargetName) is DependencyObject @object))
            {
                //throw new ArgumentException($"是因为找不到 {TargetName} 才打下划线的", origin.FindName(TargetName)?.ToString());
                return (null, null);
            }
            var (property, brush) = TryGetPropertyValue<Brush>(@object, Panel.BackgroundProperty, Shape.FillProperty);
            if (brush is SolidColorBrush)
            {
                return (@object, property.Name);
            }
            return (null, null);
        }

        /// <summary>
        /// 按照 <paramref name="properties"/> 参数列表中指定的各种依赖项属性依次尝试从 <paramref name="d"/> 中获取值。
        /// 一旦获取到，则返回此时用于获取值的依赖属性和其值。
        /// </summary>
        /// <typeparam name="T">要获取的依赖属性值的类型，如果不是此类型，即便获取到了值，也认为未获取到。</typeparam>
        /// <param name="d">要查找属性的依赖对象。</param>
        /// <param name="properties">要查找的依赖项属性数组。</param>
        /// <returns>查找到值的依赖属性和其对应的值。</returns>
        private (DependencyProperty property, T value) TryGetPropertyValue<T>(
            DependencyObject d, params DependencyProperty[] properties)
        {
            if (d == null) throw new ArgumentNullException(nameof(d));
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            foreach (var property in properties)
            {
                var value = d.GetValue(property);
                if (value is T t)
                {
                    return (property, t);
                }
            }
            return (null, default(T));
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
