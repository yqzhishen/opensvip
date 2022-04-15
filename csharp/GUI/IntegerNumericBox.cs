using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenSvip.GUI
{
    /// <summary>
    /// 作者：静游者
    /// 来源：https://www.cnblogs.com/dingshengtao/p/6274116.html
    /// </summary>
    public class IntegerNumericBox : NumericBox<int>
    {
        #region DependencyProperty
        private const int CURVALUE = 0; //当前值
        private const int MINVALUE = int.MinValue; //最小值
        private const int MAXVALUE = int.MaxValue; //最大值

        static IntegerNumericBox()
        {
            var metadata = new FrameworkPropertyMetadata(CURVALUE, OnCurValueChanged);
            CurValueProperty = DependencyProperty.Register("CurValue", typeof(int), typeof(IntegerNumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(MINVALUE, OnMinValueChanged);
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(IntegerNumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(MAXVALUE, OnMaxValueChanged);
            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(IntegerNumericBox), metadata);
        }
        #endregion

        protected override void NumericBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var numericBox = (IntegerNumericBox) sender;
            if (string.IsNullOrEmpty(numericBox.Text))
            {
                return;
            }

            TrimZeroStart();

            if (!int.TryParse(numericBox.Text, out var value))
            {
                return;
            }

            if (value != CurValue && numericBox.Text.Last() != '.')
            {
                CurValue = value;
            }
        }

        protected override void NumericBox_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            if (IsControlKeys(key) || IsDigit(key))
            {
                return;
            }
            if (IsSubtract(key)) //-
            {
                var textBox = (TextBox) sender;
                var str = textBox.Text;
                if (str.Length > 0 && textBox.SelectionStart != 0)
                {
                    e.Handled = true;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        protected override void NumericBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var numericBox = (IntegerNumericBox) sender;
            if (string.IsNullOrEmpty(numericBox.Text))
            {
                numericBox.Text = CurValue.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
