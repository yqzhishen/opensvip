using System;
using OpenSvip.Model;
using SynthV.Model;

namespace Plugin.SynthV
{
    public class SynthVDecoder
    {
        public Project DecodeProject(SVProject svProject)
        {
            var project = new Project();
            var time = svProject.Time;
            foreach (var meter in time.Meters)
            {
                project.TimeSignatureList.Add(DecodeMeter(meter));
            }

            foreach (var tempo in time.Tempos)
            {
                project.SongTempoList.Add(DecodeTempo(tempo));
            }
            return project;
        }

        private TimeSignature DecodeMeter(SVMeter meter)
        {
            return new TimeSignature
            {
                BarIndex = meter.Index,
                Numerator = meter.Numerator,
                Denominator = meter.Denominator
            };
        }

        private SongTempo DecodeTempo(SVTempo tempo)
        {
            return new SongTempo
            {
                Position = DecodePosition(tempo.Position),
                BPM = (float) tempo.BPM
            };
        }

        private Track DecodeTrack(SVTrack track)
        {
            Track svipTrack;
            if (track.MainRef.IsInstrumental)
            {
                svipTrack = new InstrumentalTrack
                {
                    AudioFilePath = track.MainRef.Audio.Filename,
                    Offset = DecodePosition(track.MainRef.BlickOffset)
                };
            }
            else
            {
                var singingTrack = new SingingTrack
                {
                    // TODO: singing track attributes
                };
                svipTrack = singingTrack;
            }
            svipTrack.Title = track.Name;
            svipTrack.Mute = track.Mixer.Mute;
            svipTrack.Solo = track.Mixer.Solo;
            svipTrack.Volume = DecodeVolume(track.Mixer.GainDecibel);
            svipTrack.Pan = track.Mixer.Pan;
            return svipTrack;
        }

        private double DecodeVolume(double gain)
        {
            return gain >= 0
                ? Math.Min(gain / (20 * Math.Log10(4)) + 1.0, 2.0)
                : Math.Pow(10, gain / 20.0);
        }

        private int DecodePosition(long position)
        {
            return (int) Math.Round(position / 1470000.0);
            // TODO: is this right??
        }
    }
}
