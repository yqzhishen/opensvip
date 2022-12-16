using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace BinSvip.Standalone
{
    public static class Singers
    {
        private static readonly Dictionary<string, string> SingerNames;

        static Singers()
        {
            var stream = new FileStream(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SingerDict.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            SingerNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
        }

        public static string GetName(string id)
        {
            if (SingerNames.ContainsKey(id))
            {
                return SingerNames[id];
            }

            return Regex.IsMatch(id, "[FM]\\d+") ? $"$({id})" : "";
        }

        public static string GetId(string name)
        {
            foreach (var singer in SingerNames.Where(singer => singer.Value.Equals(name)))
            {
                return singer.Key;
            }

            return Regex.IsMatch(name, "\\$\\([FM]\\d+\\)") ? name.Substring(2, name.Length - 3) : "";
        }
    }

    public static class ReverbPresets
    {
        private static readonly Dictionary<Model.ReverbPreset, string> Presets
            = new Dictionary<Model.ReverbPreset, string>
            {
                { Model.ReverbPreset.NONE, "干声" },
                { Model.ReverbPreset.DEFAULT, "浮光" },
                { Model.ReverbPreset.SMALLHALL1, "午后" },
                { Model.ReverbPreset.MEDIUMHALL1, "月光" },
                { Model.ReverbPreset.LARGEHALL1, "水晶" },
                { Model.ReverbPreset.SMALLROOM1, "汽水" },
                { Model.ReverbPreset.MEDIUMROOM1, "夜莺" },
                { Model.ReverbPreset.LONGREVERB2, "大梦" }
            };

        public static string GetName(Model.ReverbPreset index)
        {
            return Presets.ContainsKey(index) ? Presets[index] : null;
        }

        public static Model.ReverbPreset GetIndex(string name)
        {
            foreach (var preset in Presets.Where(preset => preset.Value.Equals(name)))
            {
                return preset.Key;
            }

            return Model.ReverbPreset.NONE;
        }
    }

    public static class NoteHeadTags
    {
        public static string GetName(Model.NoteHeadTag index)
        {
            switch (index)
            {
                case Model.NoteHeadTag.SilTag:
                    return "0";
                case Model.NoteHeadTag.SpTag:
                    return "V";
                case Model.NoteHeadTag.NoTag:
                default:
                    return null;
            }
        }

        public static Model.NoteHeadTag GetIndex(string name)
        {
            switch (name)
            {
                case "0":
                    return Model.NoteHeadTag.SilTag;
                case "V":
                    return Model.NoteHeadTag.SpTag;
                default:
                    return Model.NoteHeadTag.NoTag;
            }
        }
    }
}