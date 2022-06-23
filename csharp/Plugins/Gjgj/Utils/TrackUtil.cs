using FlutyDeer.GjgjPlugin.Model;
using FlutyDeer.GjgjPlugin.Optiions;
using OpenSvip.Library;
using OpenSvip.Model;
using System;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class TrackUtil
    {
        public static int FirstBarLength { get; set; }
        public static float FirstBarBPM { get; set; }
        public static bool IsUseLegacyPinyin { get; set; }
        public static TimeSynchronizer timeSync { get; set; }
        public static LyricsAndPinyinOption LyricsAndPinyinOption { get; set; }
        public static string SingerName { get; set; }
        public static int ParamSampleInterval { get; set; }

        /// <summary>
        /// 转换伴奏轨。
        /// </summary>
        /// <param name="trackID">轨道ID。</param>
        /// <param name="instrumentalTrack">原始伴奏轨。</param>
        /// <returns>转换后的伴奏轨。</returns>
        public static GjInstrumentalTrack EncodeInstrumentalTrack(int trackID, InstrumentalTrack instrumentalTrack)
        {
            GjTrackVolume gjTrackVolume = new GjTrackVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = instrumentalTrack.Mute
            };
            GjInstrumentalTrack gjInstrumentalTrack = new GjInstrumentalTrack
            {
                TrackID = Convert.ToString(trackID),
                Path = instrumentalTrack.AudioFilePath,
                Offset = EncodeInstOffset(instrumentalTrack.Offset),
                EQProgram = "",
                SortIndex = 0,
                TrackVolume = gjTrackVolume
            };
            return gjInstrumentalTrack;
        }

        private static int EncodeInstOffset(int origin)
        {
            int position = origin + FirstBarLength;
            if (position > 0)
            {
                return (int)(timeSync.GetActualSecsFromTicks(position) * 10000000);
            }
            else
            {
                return (int)(position / 480 * 60 / FirstBarBPM * 10000000);
            }
        }

        /// <summary>
        /// 转换演唱轨。
        /// </summary>
        /// <param name="noteID">音符ID。</param>
        /// <param name="trackID">轨道ID。</param>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns>转换后的演唱轨。</returns>
        public static GjSingingTrack EncodeSingingTrack(ref int noteID, int trackID, SingingTrack singingTrack)
        {
            var noteListUtil = new NoteListUtil
            {
                FirstBarLength = FirstBarLength,
                IsUseLegacyPinyin = IsUseLegacyPinyin,
                TimeSynchronizer = timeSync,
                LyricsAndPinyinOption = LyricsAndPinyinOption,
            };
            GjSingingTrack gjSingingTrack = new GjSingingTrack
            {
                TrackID = Convert.ToString(trackID),
                Type = 0,
                Name = SingerNameUtil.ToSingerCode(SingerName),
                SortIndex = 0,
                NoteList = noteListUtil.EncodeNoteList(noteID, singingTrack.NoteList),
                VolumeParam = VolumeParamUtil.EncodeVolumeParam(singingTrack, ParamSampleInterval),
                PitchParam = PitchParamUtil.EncodePitchParam(singingTrack),
                SingerInfo = new GjSingerInfo(),
                Keyboard = EncodeKeyboard(),
                TrackVolume = EncodeTrackVolume(singingTrack),
                EQProgram = "无"
            };
            return gjSingingTrack;
        }

        /// <summary>
        /// 返回轨道的Keyboard属性。
        /// </summary>
        private static GjKeyboard EncodeKeyboard()
        {
            GjKeyboard gjKeyboard = new GjKeyboard
            {
                KeyMode = 1,
                KeyType = 0
            };
            return gjKeyboard;
        }

        /// <summary>
        /// 返回演唱轨的音量属性。
        /// </summary>
        /// <param name="singingTrack">原始演唱轨。</param>
        /// <returns></returns>
        private static GjTrackVolume EncodeTrackVolume(SingingTrack singingTrack)
        {
            GjTrackVolume gjTrackVolume = new GjTrackVolume
            {
                Volume = 1.0f,
                LeftVolume = 1.0f,
                RightVolume = 1.0f,
                Mute = singingTrack.Mute
            };
            return gjTrackVolume;
        }        
    }
}
