using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace OxygenDioxide.UstxPlugin.Ustx {
    public class UNote : IComparable {
        static readonly Regex phoneticHintPattern = new Regex(@"\[(.*)\]");

        public int position;
        public int duration;
        public int tone;
        public string lyric = "a";
        public UPitch pitch;
        public UVibrato vibrato;

        [YamlIgnore] public int End => position + duration;

        public static UNote Create() {
            var note = new UNote();
            note.pitch = new UPitch();
            note.vibrato = new UVibrato();
            return note;
        }

        public int CompareTo(object obj) {
            if (obj == null) return 1;

            if (!(obj is UNote other))
                throw new ArgumentException("CompareTo object is not a Note");

            if (position != other.position) {
                return position.CompareTo(other.position);
            }
            return GetHashCode().CompareTo(other.GetHashCode());
        }

        public override string ToString() {
            return $"\"{lyric}\" Pos:{position} Dur:{duration} Tone:{tone}{(string.Empty)}{(string.Empty)}";
        }
    }

    public class UVibrato {
        // Vibrato percentage of note length.
        float _length;
        // Period in milliseconds.
        float _period = 0;
        // Depth in cents (1 semitone = 100 cents).
        float _depth = 0;
        // Fade-in percentage of vibrato length.
        float _in = 0;
        // Fade-out percentage of vibrato length.
        float _out = 0;
        // Shift percentage of period length.
        float _shift = 0;
        float _drift;

        public float length { get => _length; set => _length = Math.Max(0, Math.Min(100, value)); }
        public float period { get => _period; set => _period = Math.Max(5, Math.Min(500, value)); }
        public float depth { get => _depth; set => _depth = Math.Max(5, Math.Min(200, value)); }

        public float @in {
            get => _in;
            set {
                _in = Math.Max(0, Math.Min(100, value));
                _out = Math.Min(_out, 100 - _in);
            }
        }

        public float @out {
            get => _out;
            set {
                _out = Math.Max(0, Math.Min(100, value));
                _in = Math.Min(_in, 100 - _out);
            }
        }
        public float shift { get => _shift; set => _shift = Math.Max(0, Math.Min(100, value)); }
        public float drift { get => _drift; set => _drift = Math.Max(-100, Math.Min(100, value)); }

        [YamlIgnore] public float NormalizedStart => 1f - length / 100f;

        /// <summary>
        /// Evaluate a position on the vibrato curve.
        /// </summary>
        /// <param name="nPos">Normalized position in note length.</param>
        /// <returns>Vector2(tick, noteNum)</returns>
        public Vector2 Evaluate(float nPos, float nPeriod, UNote note)
        {
            float nStart = NormalizedStart;
            float nIn = length / 100f * @in / 100f;
            float nInPos = nStart + nIn;
            float nOut = length / 100f * @out / 100f;
            float nOutPos = 1f - nOut;
            float t = (nPos - nStart) / nPeriod + shift / 100f;
            float y = (float)Math.Sin(2 * Math.PI * t) * depth;
            if (nPos < nStart)
            {
                y = 0;
            }
            else if (nPos < nInPos)
            {
                y *= (nPos - nStart) / nIn;
            }
            else if (nPos > nOutPos)
            {
                y *= (1f - nPos) / nOut;
            }
            return new Vector2(note.position + note.duration * nPos, note.tone + y / 100f);
        }
    }

    public enum PitchPointShape {
        /// <summary>
        /// SineInOut
        /// </summary>
        io,
        /// <summary>
        /// Linear
        /// </summary>
        l,
        /// <summary>
        /// SineIn
        /// </summary>
        i,
        /// <summary>
        /// SineOut
        /// </summary>
        o
    };

    public class PitchPoint : IComparable<PitchPoint>
    {
        public float X;
        public float Y;
        public PitchPointShape shape;

        public PitchPoint() { }

        public PitchPoint(float x, float y, PitchPointShape shape = PitchPointShape.io)
        {
            X = x;
            Y = y;
            this.shape = shape;
        }

        public PitchPoint Clone()
        {
            return new PitchPoint(X, Y, shape);
        }

        public int CompareTo(PitchPoint other) { return X.CompareTo(other.X); }
    }

    public class UPitch {
        public List<PitchPoint> data = new List<PitchPoint>();
        public bool snapFirst = true;

        public void AddPoint(PitchPoint p) {
            data.Add(p);
            data.Sort();
        }

        public void RemovePoint(PitchPoint p) {
            data.Remove(p);
        }
    }
}
