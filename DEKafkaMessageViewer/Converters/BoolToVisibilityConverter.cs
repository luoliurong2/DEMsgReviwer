using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DEKafkaMessageViewer.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : BaseConverter, IValueConverter
    {
        public BoolToVisibilityConverter() : this(true) { }

        public BoolToVisibilityConverter(bool collapseWhenInvisible):base()
        {
            CollapseWhenInvisible = collapseWhenInvisible;
        }

        public bool CollapseWhenInvisible { get; set; }
        public Visibility FalseVisible
        {
            get
            {
                if (CollapseWhenInvisible)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Visible;
            return (bool)value ? Visibility.Visible : FalseVisible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            return ((Visibility)value == Visibility.Visible);
        }
    }
}
