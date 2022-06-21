using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace SynthV.Model
{
    public class SVProject
    {
        [JsonProperty("version")] public int Version { get; set; } = 113;
        [JsonProperty("time")] public SVTime Time { get; set; } = new SVTime();
        [JsonProperty("library")] public List<SVGroup> Library = new List<SVGroup>();
        [JsonProperty("tracks")] public List<SVTrack> Tracks { get; set; } = new List<SVTrack>();
        [JsonProperty("renderConfig")] public SVConfig RenderConfig { get; set; } = new SVConfig();
        [JsonProperty("instantModeEnabled")] public bool InstantModeEnabled { get; set; }
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
        [JsonProperty("dispColor")] public string DisplayColor { get; set; } = "ff7db235";
        [JsonProperty("dispOrder")] public int DisplayOrder { get; set; }
        [JsonProperty("renderEnabled")] public bool RenderEnabled { get; set; }
        [JsonProperty("mixer")] public SVMixer Mixer { get; set; } = new SVMixer();
        [JsonProperty("mainGroup")] public SVGroup MainGroup { get; set; } = new SVGroup();
        [JsonProperty("mainRef")] public SVRef MainRef { get; set; } = new SVRef();
        [JsonProperty("groups")] public List<SVRef> Groups { get; set; } = new List<SVRef>();
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

        [JsonIgnore] private List<SVNote> _notes = new List<SVNote>();

        [JsonProperty("notes")]
        public List<SVNote> Notes
        {
            get
            {
                if (_notes.Any(note => note.Onset < 0))
                {
                    _notes = _notes.Where(note => note.Onset >= 0).ToList();
                }
                return _notes;
            }
            set
            {
                _notes = value.Where(note => note.Onset >= 0).ToList();
            }
        }

        public bool IsOverlappedWith(SVGroup another)
        {
            return Notes.Any(
                selfNote => another.Notes.Any(
                    otherNote =>
                    {
                        var x = selfNote.Onset - (otherNote.Onset + otherNote.Duration);
                        var y = selfNote.Onset + selfNote.Duration - otherNote.Onset;
                        if (x == 0 || y == 0)
                        {
                            return false;
                        }
                        return (x > 0) ^ (y > 0);
                    }));
        }

        public static SVGroup operator +(SVGroup group, long blickOffset)
        {
            return new SVGroup
            {
                Name = group.Name,
                UUID = group.UUID,
                Params = group.Params + blickOffset,
                Notes = group.Notes
                    .Select(note => note + blickOffset)
                    .Where(note => note.Onset + blickOffset >= 0)
                    .ToList()
            };
        }

        public static SVGroup operator ^(SVGroup group, int pitchOffset)
        {
            return new SVGroup
            {
                Name = group.Name,
                UUID = group.UUID,
                Params = group.Params,
                Notes = group.Notes.ConvertAll(note => note ^ pitchOffset)
            };
        }
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
        [JsonProperty("toneShift")] public SVParamCurve ToneShift { get; set; } = new SVParamCurve();

        public static SVParams operator +(SVParams parameters, long offset)
        {
            return new SVParams
            {
                Pitch = parameters.Pitch + offset,
                VibratoEnvelope = parameters.VibratoEnvelope + offset,
                Loudness = parameters.Loudness + offset,
                Tension = parameters.Tension + offset,
                Breath = parameters.Breath + offset,
                Voicing = parameters.Voicing + offset,
                Gender = parameters.Gender + offset,
                ToneShift = parameters.ToneShift + offset
            };
        }
    }

    public class SVParamCurve
    {
        [JsonProperty("mode")] public string Mode { get; set; } = "linear";
        
        [JsonProperty("points")]
        [JsonConverter(typeof(SVPointListJsonConverter))]
        public List<Tuple<long, double>> Points { get; set; } = new List<Tuple<long, double>>();

        public static SVParamCurve operator +(SVParamCurve curve, long offset)
        {
            return new SVParamCurve
            {
                Mode = curve.Mode,
                Points = curve.Points.ConvertAll(point => new Tuple<long, double>(point.Item1 + offset, point.Item2))
            };
        }
    }

    public class SVNote
    {
        [JsonProperty("onset")] public long Onset { get; set; }
        [JsonProperty("duration")] public long Duration { get; set; }
        [JsonProperty("lyrics")] public string Lyrics { get; set; } = "";
        [JsonProperty("phonemes")] public string Phonemes { get; set; } = "";
        [JsonProperty("pitch")] public int Pitch { get; set; }
        [JsonProperty("attributes")] public SVNoteAttributes Attributes { get; set; } = new SVNoteAttributes();

        [JsonProperty("systemAttributes", NullValueHandling = NullValueHandling.Ignore)]
        public SVNoteAttributes MasterAttributes
        {
            get => null;
            set => MergeAttributes(value);
        }

        private void MergeAttributes(SVNoteAttributes attributes)
        {
            foreach (var field in Attributes.GetType().GetFields()
                         .Where(field => field.FieldType == typeof(double) && !field.IsLiteral))
            {
                var incomingValue = field.GetValue(attributes);
                if (double.NaN.Equals(field.GetValue(Attributes)) && !double.NaN.Equals(incomingValue))
                {
                    field.SetValue(Attributes, incomingValue);
                }
            }
        }

        public bool PitchEdited(
            bool regardDefaultVibratoAsUnedited = true,
            bool considerInstantPitchMode = true)
        {
            return Attributes.PitchEdited(regardDefaultVibratoAsUnedited, considerInstantPitchMode);
        }

        public static SVNote operator +(SVNote note, long blickOffset)
        {
            return new SVNote
            {
                Onset = note.Onset + blickOffset,
                Duration = note.Duration,
                Lyrics = note.Lyrics,
                Phonemes = note.Phonemes,
                Pitch = note.Pitch,
                Attributes = note.Attributes
            };
        }

        public static SVNote operator ^(SVNote note, int pitchOffset)
        {
            return new SVNote
            {
                Onset = note.Onset,
                Duration = note.Duration,
                Lyrics = note.Lyrics,
                Phonemes = note.Phonemes,
                Pitch = note.Pitch + pitchOffset,
                Attributes = note.Attributes
            };
        }
    }

    public class SVNoteAttributes
    {
        public const double DefaultPitchTransition = 0.0;
        public const double DefaultPitchSlide = 0.07;
        public const double DefaultPitchDepth = 0.15;
        public const double DefaultVibratoStart = 0.25;
        public const double DefaultVibratoFade = 0.2;
        public const double DefaultVibratoDepth = 1.0;
        public const double DefaultVibratoFrequency = 5.5;
        public const double DefaultVibratoPhase = 0.0;
        public const double DefaultVibratoJitter = 1.0;

        public const double SystemPitchSlide = 0.1;
        public const double SystemPitchDepth = 0.0;

        [JsonIgnore]
        public double TransitionOffset
        {
            get => double.IsNaN(tF0Offset) ? DefaultPitchTransition : tF0Offset;
            set => tF0Offset = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0Offset = double.NaN;

        [JsonIgnore]
        public double SlideLeft
        {
            get => double.IsNaN(tF0Left) ? DefaultPitchSlide : tF0Left;
            set => tF0Left = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0Left = double.NaN;

        [JsonIgnore]
        public double SlideRight
        {
            get => double.IsNaN(tF0Right) ? DefaultPitchSlide : tF0Right;
            set => tF0Right = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0Right = double.NaN;

        [JsonIgnore]
        public double DepthLeft
        {
            get => double.IsNaN(dF0Left) ? DefaultPitchDepth : dF0Left;
            set => dF0Left = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double dF0Left = double.NaN;

        [JsonIgnore]
        public double DepthRight
        {
            get => double.IsNaN(dF0Right) ? DefaultPitchDepth : dF0Right;
            set => dF0Right = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double dF0Right = double.NaN;

        [JsonIgnore]
        public double VibratoStart
        {
            get => double.IsNaN(tF0VbrStart) ? DefaultVibratoStart : tF0VbrStart;
            set => tF0VbrStart = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0VbrStart = double.NaN;

        [JsonIgnore]
        public double VibratoLeft
        {
            get => double.IsNaN(tF0VbrLeft) ? DefaultVibratoFade : tF0VbrLeft;
            set => tF0VbrLeft = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0VbrLeft = double.NaN;

        [JsonIgnore]
        public double VibratoRight
        {
            get => double.IsNaN(tF0VbrRight) ? DefaultVibratoFade : tF0VbrRight;
            set => tF0VbrRight = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0VbrRight = double.NaN;

        [JsonIgnore]
        public double VibratoDepth
        {
            get => double.IsNaN(dF0Vbr) ? DefaultVibratoDepth : dF0Vbr;
            set => dF0Vbr = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double dF0Vbr = double.NaN;

        [JsonIgnore]
        public double VibratoFrequency
        {
            get => double.IsNaN(fF0Vbr) ? DefaultVibratoFrequency : fF0Vbr;
            set => fF0Vbr = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double fF0Vbr = double.NaN;

        [JsonIgnore]
        public double VibratoPhase
        {
            get => double.IsNaN(pF0Vbr) ? DefaultVibratoPhase : pF0Vbr;
            set => pF0Vbr = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double pF0Vbr = double.NaN;

        [JsonIgnore]
        public double VibratoJitter
        {
            get => double.IsNaN(dF0Jitter) ? DefaultVibratoJitter : dF0Jitter;
            set => dF0Jitter = value;
        }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double dF0Jitter = double.NaN;

        [DefaultValue(null)]
        [JsonProperty("dur", NullValueHandling = NullValueHandling.Ignore)]
        public double[] PhoneDurations { get; set; }
        
        public void SetPhoneDuration(int index, double value)
        {
            if (PhoneDurations == null)
            {
                PhoneDurations = new double[index + 1];
                for (var i = 0; i < index; i++)
                {
                    PhoneDurations[i] = 1.0;
                }
            }
            else if (PhoneDurations.Length <= index)
            {
                var newArr = new double[index + 1];
                for (var i = 0; i < PhoneDurations.Length; i++)
                {
                    newArr[i] = PhoneDurations[i];
                }
                for (var i = PhoneDurations.Length; i < index; i++)
                {
                    newArr[i] = 1.0;
                }
                PhoneDurations = newArr;
            }
            PhoneDurations[index] = value;
        }

        public bool PitchEdited(
            bool regardDefaultVibratoAsUnedited = true,
            bool considerInstantPitchMode = true)
        {
            const double tolerance = 1e-6;

            var transitionEdited = !double.IsNaN(tF0Offset)
                                   || !double.IsNaN(tF0Left)
                                   || !double.IsNaN(tF0Right)
                                   || !double.IsNaN(dF0Left)
                                   || !double.IsNaN(dF0Right);
            if (considerInstantPitchMode)
            {
                transitionEdited &= Math.Abs(SlideLeft - SystemPitchSlide) >= tolerance
                                    || Math.Abs(SlideRight - SystemPitchSlide) >= tolerance
                                    || Math.Abs(DepthLeft - SystemPitchDepth) >= tolerance
                                    || Math.Abs(DepthRight - SystemPitchDepth) >= tolerance;
            }
            
            var vibratoEdited = VibratoDepth != 0.0;
            if (regardDefaultVibratoAsUnedited)
            {
                vibratoEdited &= !double.IsNaN(dF0Vbr)
                                 || !double.IsNaN(tF0VbrStart)
                                 || !double.IsNaN(tF0VbrLeft)
                                 || !double.IsNaN(tF0VbrRight)
                                 || !double.IsNaN(fF0Vbr)
                                 || !double.IsNaN(pF0Vbr);
            }
            
            return transitionEdited || vibratoEdited;
        }
    }

    public class SVRef
    {
        [JsonProperty("groupID")]
        public string GroupId { get; set; } = "aba7184c-14a3-4caf-a740-69d9cdc35a80";
        [JsonProperty("blickOffset")] public long BlickOffset { get; set; }
        [JsonProperty("pitchOffset")] public int PitchOffset { get; set; }
        [JsonProperty("isInstrumental")] public bool IsInstrumental { get; set; }

        [JsonProperty("systemPitchDelta")] public SVParamCurve InstantPitch { get; set; } = new SVParamCurve();
        [JsonProperty("database")] public SVDatabase Database { get; set; } = new SVDatabase();
        [JsonProperty("audio")] public SVAudio Audio { get; set; } = new SVAudio();
        [JsonProperty("dictionary")] public string Dictionary { get; set; } = "";
        [JsonProperty("voice")] public SVVoice Voice { get; set; } = new SVVoice();
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

    public class SVVoice
    {
        [JsonProperty("paramLoudness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MasterLoudness { get; set; }
        
        [JsonProperty("paramTension", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MasterTension { get; set; }
        
        [JsonProperty("paramBreathiness", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MasterBreath { get; set; }
        
        [JsonProperty("paramGender", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MasterGender { get; set; }
        
        [JsonProperty("paramToneShift", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double MasterToneShift { get; set; }

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0Left = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0Right = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double dF0Left = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double dF0Right = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0VbrStart = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0VbrLeft = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double tF0VbrRight = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double dF0Vbr = double.NaN;

        [DefaultValue(double.NaN)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public double fF0Vbr = double.NaN;

        public SVNoteAttributes ToAttributes()
        {
            var attributes = new SVNoteAttributes();
            var thisType = GetType();
            foreach (var field in attributes.GetType().GetFields()
                         .Where(field => field.FieldType == typeof(double) && !field.IsLiteral))
            {
                var thisField = thisType.GetField(field.Name);
                if (thisField != null)
                {
                    field.SetValue(attributes, thisField.GetValue(this));
                }
            }
            return attributes;
        }
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
