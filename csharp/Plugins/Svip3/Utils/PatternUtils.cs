using FlutyDeer.Svip3Plugin.Model;
using NAudio.Wave;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Utils
{
    public static class PatternUtils
    {
        public static Tuple<int, int> GetVisiableRange(Xs3SingingPattern pattern)
        {
            int left = pattern.ClipPosition + pattern.OriginalStartPosition;
            int right = left + pattern.ClippedDuration;
            return new Tuple<int, int>(left, right);
        }


        #region Encoding
        public static List<Xs3SingingPattern> Encode(SingingTrack track)
        {
            return new List<Xs3SingingPattern>
            {
                EncodeSingingPattern(track)
            };
        }

        private static Xs3SingingPattern EncodeSingingPattern(SingingTrack track)
        {
            var lastNote = track.NoteList.Last();
            int lastNoteEndPos = lastNote.StartPos + lastNote.Length;
            var pattern = new Xs3SingingPattern
            {
                OriginalStartPosition = 0,
                ClipPosition = 0,
                OriginalDuration = lastNoteEndPos,
                ClippedDuration = lastNoteEndPos
            };
            pattern.NoteList.AddRange(new NoteListUtils().Encode(track.NoteList));
            return pattern;
        }

        public static List<Xs3AudioPattern> Encode(InstrumentalTrack track)
        {
            return new List<Xs3AudioPattern>
            {
                EncodeAudioPattern(track)
            };
        }

        private static Xs3AudioPattern EncodeAudioPattern(InstrumentalTrack track)
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
                var pattern = new Xs3AudioPattern
                {
                    OriginalDuration = audioDurationInTicks,
                    ClippedDuration = audioDurationInTicks
                };
                if (track.Offset >= 0)
                {
                    pattern.ClipPosition = 0;
                    pattern.OriginalStartPosition = (int)Math.Round(synchronizer.GetActualTicksFromTicks(track.Offset));
                }
                else
                {
                    pattern.ClipPosition = pattern.OriginalDuration - track.Offset;
                    pattern.ClipPosition = track.Offset;
                    pattern.OriginalStartPosition = -track.Offset;
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
