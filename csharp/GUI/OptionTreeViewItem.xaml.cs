using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace OpenSvip.GUI
{
    /// <summary>
    /// OptionTreeViewItem.xaml 的交互逻辑
    /// </summary>
    public partial class OptionTreeViewItem : UserControl
    {
        public OptionTreeViewItem()
        {
            InitializeComponent();
        }

        private void OptionValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Keyboard.ClearFocus();
                ElementsHelper.FindParent<Card>((TextBox)sender).Focus();
            }
        }
    }

    public class OptionTreeViewItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringTemplate { get; set; }

        public DataTemplate IntegerTemplate { get; set; }

        public DataTemplate DoubleTemplate { get; set; }

        public DataTemplate BooleanTemplate { get; set; }

        public DataTemplate EnumTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            try // prevent the designing view from throwing exceptions
            {
                if (container == null)
                {
                    return BooleanTemplate;
                }
                var option = ElementsHelper.FindParent<OptionTreeViewItem>(container).DataContext as OptionViewModel;
                switch (option.OptionInfo.Type)
                {
                    case "string":
                        return StringTemplate;
                    case "integer":
                        return IntegerTemplate;
                    case "double":
                        return DoubleTemplate;
                    case "boolean":
                        return BooleanTemplate;
                    case "enum":
                        return EnumTemplate;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
