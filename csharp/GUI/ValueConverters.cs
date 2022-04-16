using OpenSvip.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OpenSvip.GUI
{
    public class NotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class IndexToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value >= 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    public class EnumerableIsNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IEnumerable)value).Cast<object>().Any();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ValueEqualsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0].Equals(values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class CollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var integer = ((IList<TaskViewModel>) value).Count;
            return integer > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    public class AnyTrueAsVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var v in values)
            {
                if ((bool)v)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class ReversedVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }

    public class CollectionElementConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value[1] < 0)
                return null;
            return ((IList<Plugin>)value[0])[(int)value[1]];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class MenuItemIsCheckedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedIndex = (int)values[0];
            var myself = (MenuItem)values[1];
            var parent = (MenuItem)values[2];
            var items = parent.Items.Cast<object>().ToArray();
            return selectedIndex == Array.IndexOf(items, myself);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class IndexToEnumValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumValues = ((Type)parameter).GetEnumValues().OfType<object>().ToArray();
            return Array.IndexOf(enumValues, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Type)parameter).GetEnumValues().OfType<object>().ElementAt((int)value);
        }
    }

    public class EnumValueDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fields = value.GetType().GetFields().Skip(1).ToArray();
            var enumValues = value.GetType().GetEnumValues().OfType<object>().ToArray();
            return fields[Array.IndexOf(enumValues, value)].GetCustomAttribute<DescriptionAttribute>().Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanNotOperationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanOrOperationConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in value)
            {
                if ((bool)item)
                {
                    return true;
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BooleanSwitchOperationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var choices = ((string)parameter).Split(';');
            var result = (bool)value ? choices[0] : choices[1];
            if (targetType.Equals(typeof(int)))
            {
                if (!int.TryParse(result.ToString(), out int val))
                {
                    return DependencyProperty.UnsetValue;
                }
                return val;
            }
            else if (targetType.Equals(typeof(double)))
            {
                if (!double.TryParse(result.ToString(), out double val))
                {
                    return DependencyProperty.UnsetValue;
                }
                return val;
            }
            if (!result.GetType().Equals(targetType) && !result.GetType().IsSubclassOf(targetType))
            {
                return DependencyProperty.UnsetValue;
            }
            return string.Empty == result ? null : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public class PluginToFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var plugin = (Plugin)value;
            return $"{plugin.Format} (*.{plugin.Suffix})";
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class DoubleConstantSubConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Max(0, (double)value - double.Parse(parameter.ToString()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class IntegerConstantEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == int.Parse(parameter.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }

    public class DoubleSubConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            return Math.Max(0, (double)value[0] - (double)value[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
