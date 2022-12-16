using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OpenSvip.Stream.Standalone.Const
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
        private static readonly Dictionary<XSAppModel.XStudio.ReverbPreset, string> Presets
            = new Dictionary<XSAppModel.XStudio.ReverbPreset, string>
            {
                { XSAppModel.XStudio.ReverbPreset.NONE, "干声" },
                { XSAppModel.XStudio.ReverbPreset.DEFAULT, "浮光" },
                { XSAppModel.XStudio.ReverbPreset.SMALLHALL1, "午后" },
                { XSAppModel.XStudio.ReverbPreset.MEDIUMHALL1, "月光" },
                { XSAppModel.XStudio.ReverbPreset.LARGEHALL1, "水晶" },
                { XSAppModel.XStudio.ReverbPreset.SMALLROOM1, "汽水" },
                { XSAppModel.XStudio.ReverbPreset.MEDIUMROOM1, "夜莺" },
                { XSAppModel.XStudio.ReverbPreset.LONGREVERB2, "大梦" }
            };

        public static string GetName(XSAppModel.XStudio.ReverbPreset index)
        {
            return Presets.ContainsKey(index) ? Presets[index] : null;
        }

        public static XSAppModel.XStudio.ReverbPreset GetIndex(string name)
        {
            foreach (var preset in Presets.Where(preset => preset.Value.Equals(name)))
            {
                return preset.Key;
            }

            return XSAppModel.XStudio.ReverbPreset.NONE;
        }
    }

    public static class NoteHeadTags
    {
        public static string GetName(XSAppModel.XStudio.NoteHeadTag index)
        {
            switch (index)
            {
                case XSAppModel.XStudio.NoteHeadTag.SilTag:
                    return "0";
                case XSAppModel.XStudio.NoteHeadTag.SpTag:
                    return "V";
                case XSAppModel.XStudio.NoteHeadTag.NoTag:
                default:
                    return null;
            }
        }

        public static XSAppModel.XStudio.NoteHeadTag GetIndex(string name)
        {
            switch (name)
            {
                case "0":
                    return XSAppModel.XStudio.NoteHeadTag.SilTag;
                case "V":
                    return XSAppModel.XStudio.NoteHeadTag.SpTag;
                default:
                    return XSAppModel.XStudio.NoteHeadTag.NoTag;
            }
        }
    }
}