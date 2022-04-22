using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace OpenSvip.GUI
{
    /// <summary>
    /// 用于实现链式转换器。<br/>
    /// 原作者：Gareth Evans<br/>
    /// 参考链接：https://stackoverflow.com/questions/2607490/is-there-a-way-to-chain-multiple-value-converters-in-xaml
    /// </summary>
    public class ValueConverterGroup : List<IValueConverter>, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
