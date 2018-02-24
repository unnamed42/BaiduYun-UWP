using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BaiduYun.Converters {

    class IntToVisibility : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language) {
            return (int)value == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return (Visibility)value == Visibility.Visible ? 1 : 0;
        }
    };

    class BoolToVisibility : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return (Visibility)value == Visibility.Visible;
        }
    };
}
