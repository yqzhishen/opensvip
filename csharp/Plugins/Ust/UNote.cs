using System;
using System.Collections.Generic;
using System.Linq;

namespace OxygenDioxide.UstPlugin.Stream {
    public class UVibrato
    {
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

        public float @in
        {
            get => _in;
            set
            {
                _in = Math.Max(0, Math.Min(100, value));
                _out = Math.Min(_out, 100 - _in);
            }
        }

        public float @out
        {
            get => _out;
            set
            {
                _out = Math.Max(0, Math.Min(100, value));
                _in = Math.Min(_in, 100 - _out);
            }
        }
        public float shift { get => _shift; set => _shift = Math.Max(0, Math.Min(100, value)); }
        public float drift { get => _drift; set => _drift = Math.Max(-100, Math.Min(100, value)); }
    }

    public enum PitchPointShape
    {
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

    public class UPitch
    {
        public List<PitchPoint> data = new List<PitchPoint>();
        public bool snapFirst = true;

        public void AddPoint(PitchPoint p)
        {
            data.Add(p);
            data.Sort();
        }

        public void RemovePoint(PitchPoint p)
        {
            data.Remove(p);
        }
        public UPitch Clone()
        {
            var result = new UPitch()
            {
                data = data.Select(p => p.Clone()).ToList(),
                snapFirst = snapFirst,
            };
            return result;
        }
    }
}