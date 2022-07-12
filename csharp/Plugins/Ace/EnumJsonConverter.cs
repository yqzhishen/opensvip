using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace AceStdio.Stream
{
    public class EnumJsonConverter<T> : JsonConverter<T> where T : Enum
    {
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            writer.WriteValue(GetDescriptionOrName(value));
        }

        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = reader.Value?.ToString();
            if (value == null)
            {
                return default;
            }
            return typeof(T).GetEnumValues()
                .Cast<T>()
                .FirstOrDefault(e => value == GetDescriptionOrName(e));
        }

        private static string GetDescriptionOrName(T value)
        {
            var name = value.ToString();
            var desc = (DescriptionAttribute) typeof(T).GetField(name)?.GetCustomAttribute(typeof(DescriptionAttribute));
            return desc?.Description ?? name;
        }
    }
}
