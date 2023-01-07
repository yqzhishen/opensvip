using FlutyDeer.Svip3Plugin.Model;
using NAudio.Wave;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
            if (lastNoteEndPos > TrackListUtils.SongDuration)
                TrackListUtils.SongDuration = lastNoteEndPos + 1920;
            return new Xs3SingingPattern
            {
                Name = "未命名",
                OriginalStartPosition = 0,
                ClipPosition = 0,
                OriginalDuration = lastNoteEndPos + 19200,
                ClippedDuration = lastNoteEndPos + 1920,
                Mute = false,
                PitchParam = PitchParamUtils.Encode(track.EditedParams.Pitch),
                NoteList = new NoteListUtils().Encode(track.NoteList)
            };
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
            var pattern = new Xs3AudioPattern
            {
                AudioFilePath = track.AudioFilePath,
                Name = Path.GetFileNameWithoutExtension(track.AudioFilePath)
            };
            var synchronizer = TempoUtils.Synchronizer;
            try
            {
                using (var reader = new AudioFileReader(track.AudioFilePath))
                {
                    audioDurationInSecs = reader.TotalTime.TotalSeconds;
                }
                int audioDurationInTicks = (int)Math.Round(synchronizer.GetActualTicksFromSecs(audioDurationInSecs));
                pattern.OriginalDuration = audioDurationInTicks;
                pattern.ClippedDuration = audioDurationInTicks;
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
                int patternEndPos = pattern.OriginalStartPosition + pattern.ClippedDuration;
                if (patternEndPos > TrackListUtils.SongDuration)
                    TrackListUtils.SongDuration = patternEndPos + 1920;
            }
            catch
            {
                Warnings.AddWarning($"无法读取\"{track.AudioFilePath}\"。", type:WarningTypes.Others);
            }
            return pattern;
        }

        #endregion
    }
}
