using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace OpenSvip.Const;

public static class Singers
{
    private static readonly Dictionary<string, string> SingerNames;
    static Singers()
    {
        var stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "OpenSvip.Const.Singers.json", FileMode.Open, FileAccess.Read);
        var reader = new StreamReader(stream, Encoding.UTF8);
        SingerNames = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
        stream.Close();
        reader.Close();
        stream.Dispose();
        reader.Dispose();
    }

    public static string GetName(string id)
    {
        if (SingerNames.ContainsKey(id))
        {
            return SingerNames[id];
        }
        if (Regex.IsMatch(id, "[FM]\\d+"))
        {
            return $"$({id})";
        }
        return "";
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
    private static readonly Dictionary<SingingTool.Library.Audio.ReverbPreset, string> Presets = new()
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
        return Presets.ContainsKey(index) ? Presets[index] : null;
    }

    public static SingingTool.Library.Audio.ReverbPreset GetIndex(string name)
    {
        foreach (var preset in Presets.Where(preset => preset.Value.Equals(name)))
        {
            return preset.Key;
        }
        return SingingTool.Library.Audio.ReverbPreset.NONE;
    }
}

public static class NoteHeadTags
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
        return name switch
        {
            "0" => SingingTool.Model.NoteHeadTag.SilTag,
            "V" => SingingTool.Model.NoteHeadTag.SpTag,
            _ => SingingTool.Model.NoteHeadTag.NoTag
        };
    }
}
