using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenSvip.GUI
{
    public abstract class NumericBox<T> : TextBox where T : IComparable
    {
        #region DependencyProperty

        public static DependencyProperty CurValueProperty;
        public static DependencyProperty MinValueProperty;
        public static DependencyProperty MaxValueProperty;

        public T CurValue
        {
            get => (T)GetValue(CurValueProperty);
            set
            {
                var v = value;
                if (value.CompareTo(MinValue) < 0)
                {
                    v = MinValue;
                }
                else if (value.CompareTo(MaxValue) > 0)
                {
                    v = MaxValue;
                }

                SetValue(CurValueProperty, v);
                // if do not go into OnCurValueChanged then force update ui
                if (!v.Equals(value))
                {
                    Text = v.ToString();
                }
            }
        }
        public T MinValue
        {
            get => (T)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }
        public T MaxValue
        {
            get => (T)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        protected static void OnCurValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var value = (T)e.NewValue;
            var numericBox = (NumericBox<T>)sender;
            numericBox.Text = value.ToString();
        }
        
        protected static void OnMinValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var minValue = (T)e.NewValue;
            var numericBox = (NumericBox<T>)sender;
            numericBox.MinValue = minValue;
        }
        protected static void OnMaxValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var maxValue = (T)e.NewValue;
            var numericBox = (NumericBox<T>)sender;
            numericBox.MaxValue = maxValue;
        }
        #endregion

        protected NumericBox()
        {
            TextChanged += NumericBox_TextChanged;
            PreviewKeyDown += NumericBox_KeyDown;
            LostFocus += NumericBox_LostFocus;
            DataObject.AddPastingHandler(this, NumericBox_Pasting);
        }

        protected abstract void NumericBox_TextChanged(object sender, TextChangedEventArgs e);

        protected abstract void NumericBox_KeyDown(object sender, KeyEventArgs e);

        protected abstract void NumericBox_LostFocus(object sender, RoutedEventArgs e);

        protected void NumericBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
        }

        private static readonly HashSet<Key> _controlKeys = new HashSet<Key>
        {
            Key.Back,
            Key.CapsLock,
            Key.Down,
            Key.End,
            Key.Enter,
            Key.Escape,
            Key.Home,
            Key.Insert,
            Key.Left,
            Key.PageDown,
            Key.PageUp,
            Key.Right,
            Key.Tab,
            Key.Up
        };

        protected static bool IsControlKeys(Key key)
        {
            return _controlKeys.Contains(key);
        }

        protected static bool IsDigit(Key key)
        {
            var shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool retVal;
            if (key >= Key.D0 && key <= Key.D9 && !shiftKey)
            {
                retVal = true;
            }
            else
            {
                retVal = key >= Key.NumPad0 && key <= Key.NumPad9;
            }
            return retVal;
        }

        protected static bool IsDot(Key key)
        {
            var shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            var flag = key == Key.Decimal || key == Key.OemPeriod && !shiftKey;
            return flag;
        }

        protected static bool IsSubtract(Key key)
        {
            var shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            var flag = key == Key.Subtract || key == Key.OemMinus && !shiftKey;
            return flag;
        }

        protected void TrimZeroStart()
        {
            if (Text.Length == 1)
            {
                return;
            }
            var resultText = Text;
            var zeroCount = 0;
            foreach (var c in Text)
            {
                if (c == '0') { zeroCount++; }
                else { break; }
            }
            if (zeroCount == 0)
            {
                return;
            }

            if (Text.Contains('.'))
            {
                if (Text[zeroCount] != '.')
                {
                    resultText = Text.TrimStart('0');
                }
                else if (zeroCount > 1)
                {
                    resultText = Text.Substring(zeroCount - 1);
                }
            }
            else if (zeroCount > 0)
            {
                resultText = Text.TrimStart('0');
            }

            Text = resultText;
        }
    }
}
