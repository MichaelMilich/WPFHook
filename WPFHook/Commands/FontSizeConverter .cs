using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace WPFHook.Commands
{
    class FontSizeConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is double)
            {
                double actualHeight = (double)values[0];
                if (values[1] is int)
                {
                    int count = 2*((int)values[1] + 1);
                    double rval = actualHeight / count;

                    if (targetType == typeof(Thickness))
                    {
                        return new Thickness(rval, 0, 0, 0);
                    }
                    else
                    {
                        return rval;
                    }
                }
                else
                    return actualHeight / 10;
            }
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
