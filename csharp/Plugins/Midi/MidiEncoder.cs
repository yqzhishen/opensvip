using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using OpenSvip.Model;
using MidiTimeSignature = Melanchall.DryWetMidi.Interaction.TimeSignature;

namespace FlutyDeer.MidiPlugin
{
    public class MidiEncoder
    {
        /// <summary>
        /// 移调。
        /// </summary>
        public int Transpose { get; set; }

        /// <summary>
        /// 是否导出歌词。
        /// </summary>
        public bool IsExportLyrics { get; set; }

        /// <summary>
        /// 是否使用歌词兼容性模式，默认为否。
        /// </summary>
        public bool IsUseCompatibleLyric { get; set; }

        /// <summary>
        /// 是否移除歌词中的常见标点符号，默认为是。
        /// </summary>
        public bool IsRemoveSymbols { get; set; }

        /// <summary>
        /// 歌词文本编码。
        /// </summary>
        public LyricEncodings LyricEncoding { get; set; }

        /// <summary>
        /// 拖拍前移补偿量，单位为梯。
        /// </summary>
        public int PreShift { get; set; }

        /// <summary>
        /// PPQ默认为480。
        /// </summary>
        public int PPQ { get; set; }

        private Project osProject;

        private MidiFile midiFile = new MidiFile();

        private MidiEventsUtil midiEventsUtil = new MidiEventsUtil();

        /// <summary>
        /// 转换为 MIDI 文件。
        /// </summary>
        /// <param name="project">原始的 OpenSvip 工程。</param>
        /// <param name="path">输出路径。</param>
        public void EncodeMidiFile(Project project, string path)
        {
            osProject = project;
            TicksPerQuarterNoteTimeDivision timeDivision = new TicksPerQuarterNoteTimeDivision((short)PPQ);
            midiFile.TimeDivision = timeDivision;//设置时基。
            //导出曲速和拍号
            midiFile.Chunks.Add(EncodeTempoTrackChunk());
            using (TempoMapManager tempoMapManager = midiFile.ManageTempoMap())
            {
                tempoMapManager.ClearTempoMap();//暂不清楚为什么一定要写入一个含有SetTempo事件的Chunk之后TempoMapManager才能正常工作。
                foreach (var tempo in osProject.SongTempoList)
                {
                    tempoMapManager.SetTempo(tempo.Position, new Tempo(midiEventsUtil.BPMToMicrosecondsPerQuarterNote((long)tempo.BPM)));
                }
                foreach (var timeSignature in osProject.TimeSignatureList)
                {
                    tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(timeSignature.BarIndex), new MidiTimeSignature(timeSignature.Numerator, timeSignature.Denominator));
                }
            }
            EncodeMidiChunks();//相当于 MIDI 里面的轨道。
            WritingSettings settings = new WritingSettings
            {
                TextEncoding = EncodingUtil.GetEncoding(LyricEncoding)
            };
            midiFile.Write(path, true, settings: settings);
        }

        /// <summary>
        /// 生成曲速轨。
        /// </summary>
        /// <returns>含有曲速事件数组的 Track Chunk。</returns>
        private TrackChunk EncodeTempoTrackChunk()
        {
            TrackChunk tempoTrackChunk = new TrackChunk(midiEventsUtil.SongTempoListToMidiEvents(osProject.SongTempoList));
            return new TrackChunk();
        }

        /// <summary>
        /// 生成 MIDI Chunks。
        /// </summary>
        private void EncodeMidiChunks()
        {
            foreach (var trackChunk in EncodeTrackChunkList())
            {
                midiFile.Chunks.Add(trackChunk);
            }
        }

        /// <summary>
        /// 生成 MIDI Track Chuck 列表
        /// </summary>
        /// <returns>含有曲速轨道和演唱轨的 Track Chuck 列表。</returns>
        private List<TrackChunk> EncodeTrackChunkList()
        {
            List<TrackChunk> trackChunkList = new List<TrackChunk>();

            foreach (var track in osProject.TrackList)
            {
                switch (track)
                {
                    case SingingTrack singingTrack:
                        trackChunkList.Add(new MidiEventsUtil{
                            IsExportLyrics = IsExportLyrics,
                            IsUseCompatibleLyric = IsUseCompatibleLyric,
                            IsRemoveSymbols = IsRemoveSymbols,
                            SemivowelPreShift = PreShift,
                            Transpose = Transpose
                        }.SingingTrackToMidiTrackChunk(singingTrack));
                        break;
                    default:
                        break;
                }
            }
            return trackChunkList;
        }
    }
}
