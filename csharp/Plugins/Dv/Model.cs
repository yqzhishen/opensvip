using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OxygenDioxide.DvPlugin.Model
{
    public class DvPoint
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
    }

    public class DvTempo
    {
        public Int32 position { get; set; }
        public UInt32 bpm { get; set; } // unit: 1/100 bpm
    }

    public class DvTimeSignature
    {
        public Int32 measure_position { get; set; }
        public UInt32 numerator { get; set; }
        public UInt32 denominator { get; set; }
    }

    public class DvNoteParameter
    {
        public List<DvPoint> amplitude_points { get; set; }
        public List<DvPoint> frequency_points { get; set; }
        public List<DvPoint> vibrato_points { get; set; }
    }

    public class DvPhoneme
    {
        public byte unknown_1 { get; set; }
        public float consonant_rate { get; set; }
        public byte vowel_modified { get; set; }
        public float medial { get; set; }
        public float rime { get; set; }
        public float ending { get; set; }
    }

    public class DvNote
    {
        public Int32 start { get; set; }
        public Int32 duration { get; set; }
        public Int32 key { get; set; }
        public Int32 vibrato { get; set; }
        public string pronunciation { get; set; }
        public string lyrics { get; set; }
        public DvNoteParameter note_vibrato_data { get; set; }
        public DvPhoneme phonemes { get; set; }
        public int ben_depth { get; set; }
        public int ben_length { get; set; }
        public int por_tail { get; set; }
        public int por_head { get; set; }
        public int timbre { get; set; }
        public string cross_lyric { get; set; }
        public int cross_timbre { get; set; }
    }

    public class DvSegment
    {
        public int start { get; set; }
        public int length { get; set; }
        public string name { get; set; }
        public string singer_name { get; set; }
        public List<DvNote> notes { get; set; }
        public List<DvPoint> volume { get; set; }
        public List<DvPoint> pitch { get; set; }
        public List<DvPoint> breath { get; set; }
        public List<DvPoint> gender { get; set; }
    }

    public class DvTrack
    {
        public string name { get; set; }
        public bool mute { get; set; }
        public bool solo { get; set; }
        public UInt32 volume { get; set; }
        public UInt32 balance { get; set; }
    }

    public class DvSingingTrack : DvTrack
    {
        public List<DvSegment> segments { get; set; }
    }

    public class DvAudioInfo
    {
        public int start { get; set; }
        public int length { get; set; }
        public string name { get; set; }
        public string path { get; set; }
    }

    public class DvAudioTrack : DvTrack
    {
        public List<DvAudioInfo> audios { get; set; }
    }

    public class DvProject
    {
        public List<DvTempo> tempos { get; set; }
        public List<DvTimeSignature> time_signatures { get; set; }
        public List<DvTrack> tracks { get; set; }

        public static DvProject Parse(BinaryReader reader)
        {
            var project = new DvProject();
            //文件头
            reader.ReadBytes(48);
            //读曲速标记
            int tempoCount = reader.ReadInt32();
            project.tempos = new List<DvTempo>(tempoCount);
            for (int i = 0; i < tempoCount; i++)
            {
                project.tempos.Add(new DvTempo
                {
                    position = reader.ReadInt32(),
                    bpm = reader.ReadUInt32()
                });
            }
            reader.ReadBytes(4);
            //读节拍标记
            int timeSignatureCount = reader.ReadInt32();
            project.time_signatures = new List<DvTimeSignature>(timeSignatureCount);
            for (int i = 0; i < timeSignatureCount; i++)
            {
                project.time_signatures.Add(new DvTimeSignature
                {
                    measure_position = reader.ReadInt32(),
                    numerator = reader.ReadUInt32(),
                    denominator = reader.ReadUInt32()
                });
            }
            //读音轨信息
            int trackCount = reader.ReadInt32();
            project.tracks = new List<DvTrack>(trackCount);
            for (int i = 0; i < trackCount; i++)
            {
                int trackType = reader.ReadInt32();
                if (trackType == 0)//合成音轨
                {
                    var track = new DvSingingTrack();
                    track.name = DvUtil.ReadPrefixedString(reader);
                    track.mute = reader.ReadBoolean();
                    track.solo = reader.ReadBoolean();
                    track.volume = reader.ReadUInt32();
                    track.balance = reader.ReadUInt32();
                    reader.ReadBytes(4);
                    int segmentCount = reader.ReadInt32();
                    track.segments = new List<DvSegment>(segmentCount);
                    for (int j = 0; j < segmentCount; j++)
                    {
                        var segment = new DvSegment();
                        segment.start = reader.ReadInt32();
                        segment.length = reader.ReadInt32();
                        segment.name = DvUtil.ReadPrefixedString(reader);
                        segment.singer_name = DvUtil.ReadPrefixedString(reader);
                        reader.ReadBytes(4);
                        int noteCount = reader.ReadInt32();
                        segment.notes = new List<DvNote>(noteCount);
                        for (int k = 0; k < noteCount; k++)
                        {
                            var note = new DvNote();
                            note.start = reader.ReadInt32();
                            note.duration = reader.ReadInt32();
                            note.key = 115 - reader.ReadInt32();
                            note.vibrato = reader.ReadInt32();
                            note.pronunciation = DvUtil.ReadPrefixedString(reader);
                            note.lyrics = DvUtil.ReadPrefixedString(reader);
                            reader.ReadBytes(1);
                            //数据块1
                            DvUtil.ReadPrefixedBytes(reader);
                            //数据块2
                            DvUtil.ReadPrefixedBytes(reader);
                            reader.ReadBytes(18);
                            note.ben_depth = reader.ReadInt32();
                            note.ben_length = reader.ReadInt32();
                            note.por_tail = reader.ReadInt32();
                            note.por_head = reader.ReadInt32();
                            note.timbre = reader.ReadInt32();
                            note.cross_lyric = DvUtil.ReadPrefixedString(reader);
                            note.cross_timbre = reader.ReadInt32();
                            segment.notes.Add(note);
                        }
                        //TODO:参数
                        DvUtil.ReadPrefixedBytes(reader);
                        DvUtil.ReadPrefixedBytes(reader);
                        DvUtil.ReadPrefixedBytes(reader);
                        DvUtil.ReadPrefixedBytes(reader);
                        DvUtil.ReadPrefixedBytes(reader);
                        DvUtil.ReadPrefixedBytes(reader);
                        DvUtil.ReadPrefixedBytes(reader);
                        track.segments.Add(segment);
                    }
                    project.tracks.Add(track);
                }
                else//伴奏音轨
                {
                    var track = new DvAudioTrack();
                    track.name = DvUtil.ReadPrefixedString(reader);
                    track.mute = reader.ReadBoolean();
                    track.solo = reader.ReadBoolean();
                    track.volume = reader.ReadUInt32();
                    track.balance = reader.ReadUInt32();
                    reader.ReadBytes(4);
                    int audioCount = reader.ReadInt32();
                    track.audios = new List<DvAudioInfo>(audioCount);
                    for (int j = 0; j < audioCount; j++)
                    {
                        var audio = new DvAudioInfo();
                        audio.start = reader.ReadInt32();
                        audio.length = reader.ReadInt32();
                        audio.name = DvUtil.ReadPrefixedString(reader);
                        audio.path = DvUtil.ReadPrefixedString(reader);
                        track.audios.Add(audio);
                    }
                    project.tracks.Add(track);
                }
            }
            return project;
        }
    }

    public class DvUtil
    {
        public static byte[] ReadPrefixedBytes(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }

        public static string ReadPrefixedString(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            return Encoding.UTF8.GetString(reader.ReadBytes(length));
        }
    }
}