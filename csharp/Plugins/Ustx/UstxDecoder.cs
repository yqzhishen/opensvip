using System;
using System.Collections.Generic;
using System.Linq;
using OpenSvip.Model;
using OxygenDioxide.UstxPlugin.Ustx;
using OxygenDioxide.UstxPlugin.Utils;
using OxygenDioxide.UstxPlugin.Options;

namespace OxygenDioxide.UstxPlugin.Stream
{
    public class UstxDecoder
    {
        public ImportPitchOption ImportPitch { get; set; }

        //ustx音轨音量转绝对音量。ustx音轨音量以分贝存储（#TODO:待实测）。超过2的音量将被削到2
        public static double DecodeVolume(double ustxVolume)
        {
            return Math.Min(Math.Pow(10, ustxVolume / 10), 2.0);
        }

        public static SongTempo DecodeTempo(UTempo ustxTempo)
        {
            int position = ustxTempo.position + 1920;
            if (ustxTempo.position == 0)
            {
                position = 0;
            }
            return new SongTempo
            {
                Position = ustxTempo.position,
                BPM = (float)ustxTempo.bpm
            };
        }

        public Project DecodeProject(UProject ustxProject)
        {
            List<SongTempo> songTempoList;
            //曲速
            if (ustxProject.tempos == null)
            {
                ustxProject.tempos = new List<UTempo> { 
                    new UTempo{
                        position = 0,
                        bpm = (float)ustxProject.bpm
                    }
                };
            }
            songTempoList = ustxProject.tempos
                    .Select(DecodeTempo)
                    .ToList();

            //节拍：默认4/4
            if (ustxProject.timeSignatures == null)
            {
                ustxProject.timeSignatures = new List<UTimeSignature> {
                    new UTimeSignature{
                        barPosition = 0,
                        beatPerBar = 4,
                        beatUnit = 4,
                    }
                };
            }
            List<TimeSignature> timeSignatureList = new List<TimeSignature> {
            new TimeSignature {
                Numerator=4,
                Denominator=4,
                BarIndex = 0,
            }};
            //音轨
            List<Track> trackList = ustxProject.tracks
                .Select(x=>(Track)DecodeTrack(x))
                .ToList();
            foreach(UTrack ustxTrack in ustxProject.tracks)
            {
                SingingTrack osTrack = DecodeTrack(ustxTrack);
                trackList.Add(DecodeTrack(ustxTrack));
            }
            //区段：OpenUTAU的音轨和区段分开存储，因此这里一个个把区段塞进音轨
            foreach(UVoicePart ustxVoicePart in ustxProject.voiceParts)
            {
                DecodeVoicePart(ustxVoicePart,(SingingTrack)trackList[ustxVoicePart.trackNo],ustxProject);
            }
            //由于OpenUTAU中伴奏音轨和合成音轨一视同仁，需要将伴奏音轨转成的空音轨删除
            trackList = trackList.FindAll(tr => ((SingingTrack)tr).NoteList.Count > 0);
            //音高曲线封尾
            if (ImportPitchOption.None != ImportPitch)
            {
                foreach (Track osTrack in trackList)
                {
                    ((SingingTrack)osTrack).EditedParams.Pitch.PointList.Add(new Tuple<int, int>(1073741823, -1));
                }
            }
            //转换伴奏区段
            foreach(UWavePart ustxWavePart in ustxProject.waveParts)
            {
                trackList.Add(DecodeWavePart(ustxWavePart, ustxProject));
            }
            Project osProject = new Project
            {
                Version = "SVIP7.0.0",
                SongTempoList = songTempoList,
                TimeSignatureList = timeSignatureList,
                TrackList = trackList
            };
            return osProject;
        }
        public SingingTrack DecodeTrack(UTrack ustxTrack)
        {
            SingingTrack osTrack = new SingingTrack
            {
                Title = "",
                Mute = ustxTrack.Mute,
                Solo = ustxTrack.Solo,
                Volume = DecodeVolume(ustxTrack.Volume),
                Pan = 0,
                AISingerName = ustxTrack.singer,
            };
            if (ImportPitchOption.None != ImportPitch)
            {
                osTrack.EditedParams.Pitch = new ParamCurve
                {
                    PointList = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(-192000,-100),
                }
                };
            }
            return osTrack;
        }
        //解析音符区段，同一个音轨上的所有音符区段合并为一个音轨
        public void DecodeVoicePart(UVoicePart ustxVoicePart, SingingTrack osTrack, UProject ustxProject)
        {
            int partOffset = ustxVoicePart.position;
            foreach (UNote ustxNote in ustxVoicePart.notes)
            {
                osTrack.NoteList.Add(DecodeNote(ustxNote, partOffset));
            }
            if (ImportPitchOption.None != ImportPitch){
                osTrack.EditedParams.Pitch.PointList.AddRange(DecodePitch(ustxVoicePart, ustxProject));
            }
            if (osTrack.Title == "")
                {
                    osTrack.Title = ustxVoicePart.name;//音轨名称采用第一个区段的名称
                }
        }
        //解析伴奏区段，每个伴奏区段独立转为一个伴奏音轨
        public static InstrumentalTrack DecodeWavePart(UWavePart ustxWavePart, UProject ustxProject)
        {
            UTrack ustxTrack = ustxProject.tracks[ustxWavePart.trackNo];//所属音轨
            InstrumentalTrack osTrack = new InstrumentalTrack {
                Title = ustxWavePart.DisplayName,
                AudioFilePath = ustxWavePart.relativePath,
                Offset = ustxWavePart.position,
                Mute = ustxTrack.Mute,
                Solo = ustxTrack.Solo,
                Pan = 0,
                Volume = DecodeVolume(ustxTrack.Volume),
            };
            return osTrack;
        }
        public static Note DecodeNote(UNote ustxNote,int partOffset)
        {
            string lyric = ustxNote.lyric;
            //OpenUTAU的连音符为+，多音节词可能还有+~、+4等形式，这里统一转为-
            if (lyric.StartsWith("+"))
            {
                lyric = "-";
            }
            Note osNote = new Note
            {
                StartPos = ustxNote.position + partOffset,
                Length = ustxNote.duration,
                KeyNumber = ustxNote.tone,
                Lyric = lyric
            };
            if(!(lyric.Length == 1 && LyricsUtil.isHanzi(lyric[0])))//如果不是单个汉字，则Pronunciation里面也写一份
            {
                osNote.Pronunciation = lyric;
            }
            return osNote;
        }
        public static List<Tuple<int,int>> DecodePitch(UVoicePart part, UProject project)
        {
            int pitchStart = BasePitchGenerator.pitchStart;
            int pitchInterval = BasePitchGenerator.pitchInterval;
            int firstBarLength = 1920;

            float[] pitches = BasePitchGenerator.BasePitch(part,project);//生成基础音高线

            var curve = part.curves.FirstOrDefault(c => c.abbr == "pitd");//PITD为手绘音高线差值。这里从ustx工程中尝试调取该参数
            if (curve != null && !curve.IsEmpty)
            {//如果参数存在
                for (int i = 0; i < pitches.Length; ++i)
                {
                    pitches[i] += curve.Sample(pitchStart + i * pitchInterval);//每个点加上PITD的值
                }
            }
            //============================================
            List<Tuple<int, int>> PointList = new List<Tuple<int, int>>
            {
            };
            PointList.Add(new Tuple<int, int>(firstBarLength + part.position,-100));
            for (int i = 0; i < pitches.Length; ++i)
            {
                PointList.Add(new Tuple<int, int>(firstBarLength + part.position + i * pitchInterval, (int)pitches[i]));
            }
            PointList.Add(new Tuple<int, int>(firstBarLength + part.position + pitches.Length * pitchInterval,-100));
            return PointList;
        }
    }
}