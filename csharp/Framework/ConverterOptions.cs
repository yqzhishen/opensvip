using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OpenSvip.Framework
{
    /// <summary>
    /// 由 OpenSVIP Framework 提供给工程转换器的转换选项。
    /// </summary>
    public class ConverterOptions
    {
        private readonly Dictionary<string, string> OptionDictionary;

        public ConverterOptions(Dictionary<string, string> dictionary)
        {
            OptionDictionary = dictionary;
        }

        /// <summary>
        /// 判断用户是否指定了某个选项。
        /// </summary>
        /// <param name="name">选项名称</param>
        public bool ContainsOption(string name)
        {
            return OptionDictionary.ContainsKey(name);
        }

        /// <summary>
        /// 取出字符串类型的选项值。
        /// </summary>
        /// <param name="name">选项名称</param>
        /// <param name="defaultValue">用户未指定该选项时，返回此默认值</param>
        public string GetValueAsString(string name, string defaultValue = null)
        {
            return OptionDictionary.ContainsKey(name) ? OptionDictionary[name] : defaultValue;
        }

        /// <summary>
        /// 取出整数类型的选项值。
        /// </summary>
        /// <param name="name">选项名称</param>
        /// <param name="defaultValue">用户未指定该选项时，返回此默认值</param>
        /// <exception cref="ArgumentException">若用户输入的选项值不是整数类型</exception>
        public int GetValueAsInteger(string name, int defaultValue = 0)
        {
            try
            {
                return OptionDictionary.ContainsKey(name) ? int.Parse(OptionDictionary[name]) : defaultValue;
            }
            catch (Exception)
            {
                throw new ArgumentException($"选项 \"{name}\" 格式不合法：应为整数。");
            }
        }
        
        /// <summary>
        /// 取出浮点数类型的选项值。
        /// </summary>
        /// <param name="name">选项名称</param>
        /// <param name="defaultValue">用户未指定该选项时，返回此默认值</param>
        /// <exception cref="ArgumentException">若用户输入的选项值不是浮点数类型</exception>
        public double GetValueAsDouble(string name, double defaultValue = 0.0)
        {
            try
            {
                return OptionDictionary.ContainsKey(name) ? double.Parse(OptionDictionary[name]) : defaultValue;
            }
            catch (Exception)
            {
                throw new ArgumentException($"选项 \"{name}\" 格式不合法：应为浮点数。");
            }
        }

        /// <summary>
        /// 取出布尔类型的选项值。
        /// </summary>
        /// <param name="name">选项名称</param>
        /// <param name="defaultValue">用户未指定该选项时，返回此默认值</param>
        /// <exception cref="ArgumentException">若用户输入的选项值不是布尔类型</exception>
        public bool GetValueAsBoolean(string name, bool defaultValue = false)
        {
            try
            {
                return OptionDictionary.ContainsKey(name) ? bool.Parse(OptionDictionary[name]) : defaultValue;
            }
            catch (Exception)
            {
                throw new ArgumentException($"选项 \"{name}\" 格式不合法：应为 true 或 false。");
            }
        }

        /// <summary>
        /// 取出枚举类型的选项值。优先使用枚举变量上注解的 Description 属性与用户输入进行匹配，其次使用枚举变量名称。匹配时忽略大小写。
        /// </summary>
        /// <param name="name">选项名称</param>
        /// <param name="defaultValue">用户未指定该选项时，返回此默认值</param>
        /// <typeparam name="E">需要返回的枚举变量类型</typeparam>
        /// <exception cref="ArgumentException">若用户的输入无法匹配任何枚举变量</exception>
        public E GetValueAsEnum<E>(string name, E defaultValue = default) where E: Enum
        {
            if (!OptionDictionary.ContainsKey(name))
            {
                return defaultValue;
            }
            var value = OptionDictionary[name];
            var enumType = typeof(E);
            var fields= enumType.GetFields().Skip(1).ToArray();
            var tags = Array.ConvertAll(fields, field => 
                field.GetCustomAttribute(typeof(DescriptionAttribute), true) is DescriptionAttribute description
                ? description.Description.ToLower()
                : field.Name.ToLower());
            for (var i = 0; i < fields.Length; i++)
            {
                if (tags[i].Equals(value.ToLower()))
                {
                    return (E) Enum.Parse(enumType, fields[i].Name);
                }
            }
            throw new ArgumentException($"选项 \"{name}\" 格式不合法：只能为 {string.Join(" 或 ", tags)}。");
        }
    }
}
