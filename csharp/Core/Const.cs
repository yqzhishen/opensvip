using System.Collections;
using System.Linq;

namespace OpenSvip
{

    namespace Const
    {
        internal static class ReverbPresets
        {
            private static readonly Hashtable Presets = new Hashtable
            {
                { SingingTool.Library.Audio.ReverbPreset.NONE, "干声" },
                { SingingTool.Library.Audio.ReverbPreset.DEFAULT, "浮光" },
                { SingingTool.Library.Audio.ReverbPreset.SMALLHALL1, "午后" },
                { SingingTool.Library.Audio.ReverbPreset.MEDIUMHALL1, "月光" },
                { SingingTool.Library.Audio.ReverbPreset.LARGEHALL1, "水晶" },
                { SingingTool.Library.Audio.ReverbPreset.SMALLROOM1, "汽水" },
                { SingingTool.Library.Audio.ReverbPreset.MEDIUMROOM1, "夜莺" },
                { SingingTool.Library.Audio.ReverbPreset.LONGREVERB2, "大梦" }
            };

            public static string GetName(SingingTool.Library.Audio.ReverbPreset index)
            {
                return (string) Presets[index];
            }

            public static SingingTool.Library.Audio.ReverbPreset GetIndex(string name)
            {
                foreach (var entry in Presets.Cast<SingingTool.Library.Audio.ReverbPreset>()
                             .Where(entry => Presets[entry].Equals(name)))
                {
                    return entry;
                }

                return SingingTool.Library.Audio.ReverbPreset.NONE;
            }
        }

        internal static class NoteHeadTags
        {
            public static string GetName(SingingTool.Model.NoteHeadTag index)
            {
                switch (index)
                {
                    case SingingTool.Model.NoteHeadTag.SilTag:
                        return "0";
                    case SingingTool.Model.NoteHeadTag.SpTag:
                        return "V";
                    case SingingTool.Model.NoteHeadTag.NoTag:
                    default:
                        return null;
                }
            }

            public static SingingTool.Model.NoteHeadTag GetIndex(string name)
            {
                switch (name)
                {
                    case "0":
                        return SingingTool.Model.NoteHeadTag.SilTag;
                    case "V":
                        return SingingTool.Model.NoteHeadTag.SpTag;
                    default:
                        return SingingTool.Model.NoteHeadTag.NoTag;
                }
            }
        }
    }
}
