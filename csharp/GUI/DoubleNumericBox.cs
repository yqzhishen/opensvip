using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenSvip.GUI
{
    public class DoubleNumericBox : NumericBox<double>
    {
        #region DependencyProperty
        private const double CURVALUE = 0; //当前值
        private const double MINVALUE = double.MinValue; //最小值
        private const double MAXVALUE = double.MaxValue; //最大值
        private const int DIGITS = 15; //小数点精度

        public static readonly DependencyProperty DigitsProperty;

        public new double CurValue
        {
            get => (double)GetValue(CurValueProperty);
            set
            {
                var v = value;
                if (value < MinValue)
                {
                    v = MinValue;
                }
                else if (value > MaxValue)
                {
                    v = MaxValue;
                }
                v = Math.Round(v, Digits);

                SetValue(CurValueProperty, v);
                // if do not go into OnCurValueChanged then force update ui
                if (Math.Abs(v - value) > 1e-6)
                {
                    Text = v.ToString(CultureInfo.InvariantCulture);
                }                
            }
        }
        public int Digits
        {
            get => (int)GetValue(DigitsProperty);
            set
            {
                var digits = value;
                if (digits <= 0)
                {
                    digits = 0;
                }
                if (digits > DIGITS)
                {
                    digits = DIGITS;
                }
                SetValue(DigitsProperty, digits);
            }
        }

        static DoubleNumericBox()
        {
            var metadata = new FrameworkPropertyMetadata(CURVALUE, OnCurValueChanged);
            CurValueProperty = DependencyProperty.Register("CurValue", typeof(double), typeof(DoubleNumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(MINVALUE, OnMinValueChanged);
            MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(DoubleNumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(MAXVALUE, OnMaxValueChanged);
            MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(DoubleNumericBox), metadata);

            metadata = new FrameworkPropertyMetadata(DIGITS, OnDigitsChanged);
            DigitsProperty = DependencyProperty.Register("Digits", typeof(int), typeof(DoubleNumericBox), metadata);
        }

        private static void OnDigitsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var digits = (int)e.NewValue;
            var numericBox = (DoubleNumericBox)sender;
            numericBox.CurValue = Math.Round(numericBox.CurValue, digits);
            numericBox.MinValue = Math.Round(numericBox.MinValue, digits);
            numericBox.MaxValue = Math.Round(numericBox.MaxValue, digits);
        }
        #endregion

        protected override void NumericBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var numericBox = (DoubleNumericBox) sender;
            if (string.IsNullOrEmpty(numericBox.Text))
            {
                return;
            }

            TrimZeroStart();

            if (!double.TryParse(numericBox.Text, out var value))
            {
                return;
            }

            if (Math.Abs(value - CurValue) > 1e-6 && !numericBox.Text.EndsWith("."))
            {
                CurValue = value;
            }
        }

        protected override void NumericBox_KeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            if (IsControlKeys(key))
            {
                return;
            }
            if (IsDigit(key))
            {
                if (Text.Contains(".") && Text.Length - Text.IndexOf('.') - 1 >= Digits)
                {
                    e.Handled = true;
                }
                return;
            }
            if (IsSubtract(key)) //-
            {
                var textBox = (DoubleNumericBox) sender;
                var str = textBox.Text;
                if (str.Length > 0 && textBox.SelectionStart != 0)
                {
                    e.Handled = true;
                }
            }
            else if (IsDot(key)) //point
            {
                if (Digits > 0)
                {
                    var textBox = (DoubleNumericBox) sender;
                    var str = textBox.Text;
                    if (str.Contains('.') || str == "-")
                    {
                        e.Handled = true;
                    }
                }
                else
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
            var numericBox = (DoubleNumericBox) sender;
            numericBox.Text = numericBox.Text.TrimEnd('.');
            if (string.IsNullOrEmpty(numericBox.Text))
            {
                numericBox.Text = CurValue.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
