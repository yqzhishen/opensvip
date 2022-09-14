using FlutyDeer.Svip3Plugin.Proto;
using ProtoBuf;
using System.Collections.Generic;

namespace FlutyDeer.Svip3Plugin.Model
{
    [ProtoContract]
    public class Xs3SingingTrack
    {
        [ProtoMember(1)]
        public float Gain { get; set; }

        [ProtoMember(2)]
        public float Pan { get; set; }

        [ProtoMember(3)]
        public bool Mute { get; set; }

        [ProtoMember(4)]
        public string Name { get; set; }

        [ProtoMember(5)]
        public bool Solo { get; set; }

        [ProtoMember(6)]
        public string Color { get; set; }

        [ProtoMember(8)]
        public List<Xs3SingingPattern> PatternList { get; set; } = new List<Xs3SingingPattern>();

        [ProtoMember(9)]
        public string SingerId { get; set; }

    }
}
