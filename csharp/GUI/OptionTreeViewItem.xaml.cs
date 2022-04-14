using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
