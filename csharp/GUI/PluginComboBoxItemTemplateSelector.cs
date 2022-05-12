using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenSvip.GUI
{
    public class PluginComboBoxItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DropDownItemTemplate { get; set; }

        public DataTemplate SelectedItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var parentItem = container;
            while (parentItem != null && !(parentItem is ComboBox) && !(parentItem is ComboBoxItem))
            {
                parentItem = VisualTreeHelper.GetParent(parentItem);
            }
            return parentItem is ComboBoxItem
                ? DropDownItemTemplate
                : SelectedItemTemplate;
        }
    }
}
