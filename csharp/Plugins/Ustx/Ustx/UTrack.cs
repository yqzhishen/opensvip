using System;
using System.Linq;
using YamlDotNet.Serialization;

namespace OxygenDioxide.UstxPlugin.Ustx {
    public class UTrack {
        public string singer;
        public string phonemizer;
        public string renderer;
        public bool Mute { set; get; }
        public bool Solo { set; get; }
        public double Volume { set; get; }
    }
}
