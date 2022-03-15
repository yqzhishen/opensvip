using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OpenSvip.Framework
{
    public class ConverterOptions
    {
        private readonly Dictionary<string, string> OptionDictionary;

        public ConverterOptions(Dictionary<string, string> dictionary)
        {
            OptionDictionary = dictionary;
        }

        public bool ContainsOption(string name)
        {
            return OptionDictionary.ContainsKey(name);
        }

        public string GetOptionAsString(string name, string defaultValue = null)
        {
            return OptionDictionary.ContainsKey(name) ? OptionDictionary[name] : defaultValue;
        }

        public int GetOptionAsInteger(string name, int defaultValue = 0)
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

        public double GetOptionAsDouble(string name, double defaultValue = 0.0)
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

        public bool GetOptionAsBoolean(string name, bool defaultValue = false)
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

        public E GetOptionAsEnum<E>(string name, E defaultValue = default) where E: Enum
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
            throw new ArgumentException($"选项 \"{name}\" 格式不合法，只能为： {string.Join(" 或 ", tags)}。");
        }
    }
}
