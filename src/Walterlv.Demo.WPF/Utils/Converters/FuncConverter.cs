using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Walterlv.Demo.Converters
{
    public sealed class FuncConverter : IValueConverter
    {
        public Func<object, object> Convert { get; set; }

        public Func<object, object> ConvertBack { get; set; }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert == null ? DependencyProperty.UnsetValue : Convert(value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ConvertBack == null)
            {
                throw new NotSupportedException();
            }
            return ConvertBack(value);
        }
    }

    public sealed class FuncConverter<TSource, TTarget> : IValueConverter
    {
        public Func<TSource, TTarget> Convert { get; set; }

        public Func<TTarget, TSource> ConvertBack { get; set; }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert == null ? DependencyProperty.UnsetValue : Convert((TSource) value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ConvertBack == null)
            {
                throw new NotSupportedException();
            }
            return ConvertBack((TTarget) value);
        }
    }
}
