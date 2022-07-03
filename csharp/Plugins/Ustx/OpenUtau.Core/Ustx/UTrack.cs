﻿using System;
using System.Linq;
using Serilog;
using YamlDotNet.Serialization;

namespace OpenUtau.Core.Ustx {
    public class UTrack {
        public string singer;
        public string phonemizer;
        public string renderer;
        public bool Mute { set; get; }
        public bool Solo { set; get; }
        public double Volume { set; get; }
    }
}
