using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace OxygenDioxide.UstxPlugin.Ustx 
{
    public class UTempo
    {
        public int position;
        public double bpm;

        public UTempo() { }
        public UTempo(int position, double bpm)
        {
            this.position = position;
            this.bpm = bpm;
        }
        public override string ToString() => $"{bpm}@{position}";
    }

    public class UTimeSignature
    {
        public int barPosition;
        public int beatPerBar;
        public int beatUnit;

        public UTimeSignature() { }
        public UTimeSignature(int barPosition, int beatPerBar, int beatUnit)
        {
            this.barPosition = barPosition;
            this.beatPerBar = beatPerBar;
            this.beatUnit = beatUnit;
        }
        public override string ToString() => $"{beatPerBar}/{beatUnit}@bar{barPosition}";
    }
    public class UProject 
    {
        public string name = "New Project";
        public string comment = string.Empty;
        public string outputDir = "Vocal";
        public string cacheDir = "UCache";
        [YamlMember(SerializeAs = typeof(string))]
        public Version ustxVersion;
        public int resolution = 480;

        [Obsolete("Since ustx v0.6")] public double bpm = 120;
        [Obsolete("Since ustx v0.6")] public int beatPerBar = 4;
        [Obsolete("Since ustx v0.6")] public int beatUnit = 4;

        public Dictionary<string, UExpressionDescriptor> expressions = new Dictionary<string, UExpressionDescriptor>();
        public List<UTimeSignature> timeSignatures;
        public List<UTempo> tempos;
        public List<UTrack> tracks;

        /// <summary>
        /// Transient field used for serialization.
        /// </summary>
        public List<UVoicePart> voiceParts;
        /// <summary>
        /// Transient field used for serialization.
        /// </summary>
        public List<UWavePart> waveParts;

        /*
        public int MillisecondToTick(double ms)
        {
            return MusicMath.MillisecondToTick(ms, bpm, beatUnit, resolution);
        }

        public double TickToMillisecond(double tick)
        {
            return MusicMath.TickToMillisecond(tick, bpm, beatUnit, resolution);
        }*/
    }
}
