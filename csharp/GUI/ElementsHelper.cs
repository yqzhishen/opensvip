using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace OpenSvip.GUI
{
    /// <summary>
    /// 来源：https://cloud.tencent.com/developer/article/1625617
    /// 作者：剑行者
    /// </summary>
    class ElementsHelper
    {
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // 如果子控件不是需查找的控件类型
                T childType = child as T;
                if (childType == null)
                {
                    // 在下一级控件中递归查找
                    foundChild = FindChild<T>(child, childName);

                    // 找到控件就可以中断递归操作 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    // 如果控件名称符合参数条件
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // 查找到了控件
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static List<T> FindChildren<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            var list = new List<T>();
            if (parent == null) return list;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // 如果子控件不是需查找的控件类型
                T childType = child as T;
                if (childType == null)
                {
                    // 在下一级控件中递归查找
                    var findChildList = FindChildren<T>(child, childName);
                    for (int j = 0; j < findChildList.Count; j++)
                    {

                    }
                    list.AddRange(FindChildren<T>(child, childName));

                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // 如果控件名称符合参数条件
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        list.Add((T)child);
                    }
                }
                else
                {
                    // 查找到了控件
                    list.Add((T)child);
                }
            }

            return list;
        }

        /// <summary>
        /// 查找父元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T FindParent<T>(DependencyObject i_dp) where T : DependencyObject
        {
            DependencyObject dobj = VisualTreeHelper.GetParent(i_dp);
            if (dobj != null)
            {
                if (dobj is T)
                {
                    return (T)dobj;
                }
                else
                {
                    dobj = FindParent<T>(dobj);
                    if (dobj != null && dobj is T t)
                    {
                        return t;
                    }
                }
            }
            return null;
        }
    }
}
