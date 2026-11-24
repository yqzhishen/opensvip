using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using OpenSvip.Model;
using MidiTimeSignature = Melanchall.DryWetMidi.Interaction.TimeSignature;
using FlutyDeer.MidiPlugin.Options;
using FlutyDeer.MidiPlugin.Utils;
using OpenSvip.Library;

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
        /// 默认歌词。
        /// </summary>
        public string DefaultLyric { get; set; }

        /// <summary>
        /// 是否使用歌词兼容性模式，默认为否。
        /// </summary>
        public bool IsUseCompatibleLyric { get; set; }


        /// <summary>
        /// 是否移除歌词中的常见标点符号，默认为是。
        /// </summary>
        public bool IsRemoveSymbols { get; set; }

        //public bool IsConstantTempo { get; set; }

        /// <summary>
        /// 拖拍前移补偿量，单位为梯。
        /// </summary>
        public int PreShift { get; set; }

        /// <summary>
        /// PPQ默认为480。
        /// </summary>
        public int PPQ { get; set; }

        //public double ConstantTempo { get; set; }

        private Project osProject;

        private MidiEventsUtil midiEventsUtil = new MidiEventsUtil();

        //private TimeSynchronizer synchronizer;

        /// <summary>
        /// 转换为 MIDI 文件。
        /// </summary>
        /// <param name="project">原始的 OpenSvip 工程。</param>
        /// <param name="path">输出路径。</param>
        public MidiFile EncodeMidiFile(Project project)
        {
            osProject = project;
            //var originalTempoList = osProject.SongTempoList;
            //var defaultTempo = osProject.SongTempoList[0].BPM;
            //synchronizer = new TimeSynchronizer(originalTempoList, isAbsoluteTimeMode: IsConstantTempo, defaultTempo: defaultTempo);
            MidiFile midiFile = new MidiFile();
            TicksPerQuarterNoteTimeDivision timeDivision = new TicksPerQuarterNoteTimeDivision((short)PPQ);
            midiFile.TimeDivision = timeDivision;//设置时基。
            //导出曲速和拍号
            midiFile.Chunks.Add(new TrackChunk());
            using (TempoMapManager tempoMapManager = midiFile.ManageTempoMap())
            {
                tempoMapManager.ClearTempoMap();
                foreach (var tempo in osProject.SongTempoList)
                {
                    tempoMapManager.SetTempo(tempo.Position, new Tempo(midiEventsUtil.BPMToMicrosecondsPerQuarterNote(tempo.BPM)));
                }
                foreach (var timeSignature in osProject.TimeSignatureList)
                {
                    tempoMapManager.SetTimeSignature(new BarBeatTicksTimeSpan(timeSignature.BarIndex),
                                                     new MidiTimeSignature(timeSignature.Numerator, timeSignature.Denominator));
                }
            }
            EncodeMidiChunks(midiFile);//相当于 MIDI 里面的轨道。
            return midiFile;
        }

        /// <summary>
        /// 生成 MIDI Chunks。
        /// </summary>
        private void EncodeMidiChunks(MidiFile midiFile)
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
                            //IsConstantTempo = IsConstantTempo,
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
