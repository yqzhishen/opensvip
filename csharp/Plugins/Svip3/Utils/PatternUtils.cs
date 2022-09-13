using Google.Protobuf.Collections;
using NAudio.Wave;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.Linq;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class PatternUtils
    {
        public static Tuple<int, int> GetVisiableRange(SingingPattern pattern)
        {
            int left = pattern.PlayPos + pattern.RealPos;
            int right = left + pattern.PlayDur;
            return new Tuple<int, int>(left, right);
        }


        #region Encoding
        public static RepeatedField<SingingPattern> Encode(OpenSvip.Model.SingingTrack track)
        {
            return new RepeatedField<SingingPattern>
            {
                EncodeSingingPattern(track)
            };
        }

        private static SingingPattern EncodeSingingPattern(OpenSvip.Model.SingingTrack track)
        {
            var lastNote = track.NoteList.Last();
            int lastNoteEndPos = lastNote.StartPos + lastNote.Length;
            var pattern = new SingingPattern
            {
                RealPos = 0,
                PlayPos = 0,
                RealDur = lastNoteEndPos,
                PlayDur = lastNoteEndPos
            };
            pattern.NoteList.AddRange(new NoteListUtils().Encode(track.NoteList));
            return pattern;
        }

        public static RepeatedField<AudioPattern> Encode(InstrumentalTrack track)
        {
            return new RepeatedField<AudioPattern>
            {
                EncodeAudioPattern(track)
            };
        }

        private static AudioPattern EncodeAudioPattern(InstrumentalTrack track)
        {
            double audioDurationInSecs;
            var synchronizer = TempoUtils.Synchronizer;
            try
            {
                using (var reader = new AudioFileReader(track.AudioFilePath))
                {
                    audioDurationInSecs = reader.TotalTime.TotalSeconds;
                }
                int audioDurationInTicks = (int)Math.Round(synchronizer.GetActualTicksFromSecs(audioDurationInSecs));
                var pattern = new AudioPattern
                {
                    RealDur = audioDurationInTicks,
                    PlayDur = audioDurationInTicks
                };
                if (track.Offset >= 0)
                {
                    pattern.PlayPos = 0;
                    pattern.RealPos = (int)Math.Round(synchronizer.GetActualTicksFromTicks(track.Offset));
                }
                else
                {
                    pattern.PlayDur = pattern.RealDur - track.Offset;
                    pattern.PlayPos = track.Offset;
                    pattern.RealPos = -track.Offset;
                }
                return pattern;
            }
            catch
            {
                Warnings.AddWarning($"无法读取\"{track.AudioFilePath}\"。", type:WarningTypes.Others);
                return null;
            }
        }

        #endregion
    }
}
