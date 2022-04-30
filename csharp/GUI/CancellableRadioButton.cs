using System.Windows;
using System.Windows.Controls;

namespace OpenSvip.GUI
{
    public class CancellableRadioButton : RadioButton
    {
        public CancellableRadioButton()
        {
            PreviewMouseDown += Self_PreviewMouseDown;
            Checked += Self_Checked;
            Unchecked += Self_UnChecked;
        }

        private bool _lastChecked;

        private void Self_PreviewMouseDown(object sender, RoutedEventArgs e)
        {
            IsChecked = !_lastChecked;
            if (!_lastChecked)
            {
                e.Handled = true;
            }
        }

        private void Self_Checked(object sender, RoutedEventArgs e)
        {
            _lastChecked = true;
        }

        private void Self_UnChecked(object sender, RoutedEventArgs e)
        {
            _lastChecked = false;
        }
    }
}
