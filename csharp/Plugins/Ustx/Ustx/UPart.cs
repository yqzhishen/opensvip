using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OxygenDioxide.UstxPlugin.Ustx {
    public abstract class UPart {
        public string name = "New Part";
        public string comment = string.Empty;
        public int trackNo;
        public int position = 0;

        [YamlIgnore] public virtual string DisplayName { get; }
        [YamlIgnore] public virtual int Duration { set; get; }
        [YamlIgnore] public int EndTick { get { return position + Duration; } }
    }

    public class UVoicePart : UPart {
        [YamlMember(Order = 100)]
        public SortedSet<UNote> notes = new SortedSet<UNote>();
        [YamlMember(Order = 101)]
        public List<UCurve> curves = new List<UCurve>();

        public override string DisplayName => name;
    }

    public class UWavePart : UPart {
        string _filePath;

        [YamlIgnore]
        public string FilePath {
            set {
                _filePath = value;
                name = Path.GetFileName(value);
            }
            get { return _filePath; }
        }

        [YamlMember(Order = 100)] public string relativePath;
        [YamlMember(Order = 101)] public double fileDurationMs;
        [YamlMember(Order = 102)] public double skipMs;
        [YamlMember(Order = 103)] public double TrimMs;

        [YamlIgnore]
        public override string DisplayName => Missing ? $"[Missing] {name}" : name;
        [YamlIgnore]
        public override int Duration {
            get => fileDurTick;
            set { }
        }
        [YamlIgnore] bool Missing { get; set; }
        [YamlIgnore] public float[] Peaks { get; set; }
        [YamlIgnore] public float[] Samples { get; private set; }

        [YamlIgnore] public int channels;
        [YamlIgnore] public int fileDurTick;

        private TimeSpan duration;
    }
}
