using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugin.SynthV;

namespace SynthV.Model
{
    public class SVProject
    {
        [JsonProperty("version")] public int Version { get; set; } = 113;
        [JsonProperty("time")] public SVTime Time { get; set; } = new SVTime();
        [JsonProperty("library")] public object[] Library = Array.Empty<object>();
        [JsonProperty("tracks")] public List<SVTrack> Tracks { get; set; } = new List<SVTrack>();
        [JsonProperty("renderConfig")] public SVConfig RenderConfig { get; set; } = new SVConfig();
    }

    public class SVTime
    {
        [JsonProperty("meter")] public List<SVMeter> Meters { get; set; } = new List<SVMeter>();
        [JsonProperty("tempo")] public List<SVTempo> Tempos { get; set; } = new List<SVTempo>();
    }

    public class SVMeter
    {
        [JsonProperty("index")] public int Index { get; set; }
        [JsonProperty("numerator")] public int Numerator { get; set; } = 4;
        [JsonProperty("denominator")] public int Denominator { get; set; } = 4;
    }

    public class SVTempo
    {
        [JsonProperty("position")] public long Position { get; set; }
        [JsonProperty("bpm")] public double BPM { get; set; }
    }

    public class SVTrack
    {
        [JsonProperty("name")] public string Name { get; set; } = "Track 1";
        [JsonProperty("dispColor")] public string DispColor { get; set; } = "ff7db235";
        [JsonProperty("dispOrder")] public int DispOrder { get; set; }
        [JsonProperty("renderEnabled")] public bool RenderEnabled { get; set; }
        [JsonProperty("mixer")] public SVMixer Mixer { get; set; } = new SVMixer();
        [JsonProperty("mainGroup")] public SVGroup MainGroup { get; set; } = new SVGroup();
        [JsonProperty("mainRef")] public SVRef MainRef { get; set; } = new SVRef();
        [JsonProperty("groups")] public List<SVGroup> Groups { get; set; } = new List<SVGroup>();
    }

    public class SVMixer
    {
        [JsonProperty("gainDecibel")] public double GainDecibel { get; set; }
        [JsonProperty("pan")] public double Pan { get; set; }
        [JsonProperty("mute")] public bool Mute { get; set; }
        [JsonProperty("solo")] public bool Solo { get; set; }
        [JsonProperty("display")] public bool Display { get; set; } = true;
    }
    
    public class SVGroup
    {
        [JsonProperty("name")] public string Name { get; set; } = "main";
        [JsonProperty("uuid")] public string UUID { get; set; } = "aba7184c-14a3-4caf-a740-69d9cdc35a80";
        [JsonProperty("parameters")] public SVParams Params = new SVParams();
        [JsonProperty("notes")] public List<SVNote> Notes = new List<SVNote>();
    }

    public class SVParams
    {
        [JsonProperty("pitchDelta")] public SVParamCurve Pitch { get; set; } = new SVParamCurve();
        [JsonProperty("vibratoEnv")] public SVParamCurve VibratoEnvelope { get; set; } = new SVParamCurve();
        [JsonProperty("loudness")] public SVParamCurve Loudness { get; set; } = new SVParamCurve();
        [JsonProperty("tension")] public SVParamCurve Tension { get; set; } = new SVParamCurve();
        [JsonProperty("breathiness")] public SVParamCurve Breath { get; set; } = new SVParamCurve();
        [JsonProperty("voicing")] public SVParamCurve Voicing { get; set; } = new SVParamCurve();
        [JsonProperty("gender")] public SVParamCurve Gender { get; set; } = new SVParamCurve();
    }

    public class SVParamCurve
    {
        [JsonProperty("mode")] public string Mode { get; set; } = "linear";
        
        [JsonProperty("points")]
        [JsonConverter(typeof(SVPointListJsonConverter))]
        public List<Tuple<long, double>> Points { get; set; } = new List<Tuple<long, double>>();
    }

    public class SVNote
    {
        [JsonProperty("onset")] public long Onset { get; set; }
        [JsonProperty("duration")] public long Duration { get; set; }
        [JsonProperty("lyrics")] public string Lyrics { get; set; } = "";
        [JsonProperty("phonemes")] public string Phonemes { get; set; } = "";
        [JsonProperty("pitch")] public int Pitch { get; set; }
        [JsonProperty("attributes")] public SVNoteAttributes Attributes { get; set; } = new SVNoteAttributes();
    }

    public class SVNoteAttributes
    {
        
    }

    public class SVRef
    {
        [JsonProperty("groupID")] public string GroupId { get; set; } = "aba7184c-14a3-4caf-a740-69d9cdc35a80";
        [JsonProperty("blickOffset")] public long BlickOffset { get; set; }
        [JsonProperty("pitchOffset")] public int PitchOffset { get; set; }
        [JsonProperty("isInstrumental")] public bool IsInstrumental { get; set; }
        [JsonProperty("database")] public SVDatabase Database { get; set; } = new SVDatabase();
        [JsonProperty("audio")] public SVAudio Audio { get; set; } = new SVAudio();
        [JsonProperty("dictionary")] public string Dictionary { get; set; } = "";
        [JsonProperty("voice")] public object Voice { get; set; } = new object();
    }

    public class SVDatabase
    {
        [JsonProperty("name")] public string Name { get; set; } = "";
        [JsonProperty("language")] public string Language { get; set; } = "";
        [JsonProperty("phoneset")] public string PhoneSet { get; set; } = "";
    }

    public class SVAudio
    {
        [JsonProperty("filename")] public string Filename { get; set; } = "";
        [JsonProperty("duration")] public double Duration { get; set; }
    }

    public class SVConfig
    {
        [JsonProperty("destination")] public string Destination { get; set; } = "./";
        [JsonProperty("filename")] public string Filename { get; set; } = "untitled";
        [JsonProperty("numChannels")] public int NumChannels { get; set; } = 1;
        [JsonProperty("aspirationFormat")] public string Aspiration { get; set; } = "noAspiration";
        [JsonProperty("bitDepth")] public int BitDepth { get; set; } = 16;
        [JsonProperty("sampleRate")] public int SampleRate { get; set; } = 44100;
        [JsonProperty("exportMixDown")] public bool ExportMixDown { get; set; } = true;
    }
}
