using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace OpenUtau.Core.Ustx {
    public class UProject {
        public string name = "New Project";
        public string comment = string.Empty;
        public string outputDir = "Vocal";
        public string cacheDir = "UCache";
        [YamlMember(SerializeAs = typeof(string))]
        public Version ustxVersion;

        public double bpm = 120;
        public int beatPerBar = 4;
        public int beatUnit = 4;
        public int resolution = 480;

        public Dictionary<string, UExpressionDescriptor> expressions = new Dictionary<string, UExpressionDescriptor>();
        public List<UTrack> tracks = new List<UTrack>();

        /// <summary>
        /// Transient field used for serialization.
        /// </summary>
        public List<UVoicePart> voiceParts;
        /// <summary>
        /// Transient field used for serialization.
        /// </summary>
        public List<UWavePart> waveParts;
    }
}
