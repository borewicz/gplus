using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace gPlus.Classes
{
    public class UserOptionsVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string costam)
        {
            string userID = (string)value;
            if (userID == Other.info.userID)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string costam)
        {
            throw new NotSupportedException();
        }
    }

    public class UserOptionsBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string costam)
        {
            string userID = (string)value;
            if (userID == Other.info.userID)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string costam)
        {
            throw new NotSupportedException();
        }
    }
}
