using FlutyDeer.Svip3Plugin.Proto;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// X Studio 3 工程文件（*.svip3）实体类
    /// </summary>
    [ProtoContract]
    public class Svip3Model
    {
        #region Properties & Fields

        [ProtoMember(1)]
        public string ProjectFilePath { get; set; }

        [ProtoMember(2)]
        public string Version { get; set; }

        [ProtoMember(3)]
        public int Duration { get; set; }

        [ProtoMember(4)]
        public List<Xs3Tempo> TempoList { get; set; } = new List<Xs3Tempo>();

        [ProtoMember(5)]
        public List<Xs3TimeSignature> TimeSignatureList { get; set; } = new List<Xs3TimeSignature>();

        [ProtoMember(6)]
        private readonly List<Any> _trackList = new List<Any>();

        public List<object> TrackList { get; set; } = new List<object>();

        [ProtoMember(7)]
        public Xs3Master Master { get; set; }

        [ProtoMember(8)]
        public string Tonality { get; set; }

        [ProtoMember(9)]
        public int QuantizeValue { get; set; }

        [ProtoMember(10)]
        public int LoopStartPos { get; set; }

        [ProtoMember(11)]
        public int LoopEndPos { get; set; }

        [ProtoMember(12)]
        public bool IsQuantize { get; set; }

        #endregion

        #region Methods

        public static Svip3Model Read(string path)
        {
            Svip3Model model;
            using (var file = File.OpenRead(path))
            {
                model = Serializer.Deserialize<Svip3Model>(file);
            }
            UnpackTracks(model);
            return model;
        }

        private static void UnpackTracks(Svip3Model model)
        {
            foreach (var any in model._trackList)
            {
                object resultTrack;
                var typeString = any.TypeUrl.Split('.').Last();
                switch (typeString)
                {
                    case "SingingTrack":
                        resultTrack = Any.Unpack<Xs3SingingTrack>(any);
                        break;
                    case "AudioTrack":
                        resultTrack = Any.Unpack<Xs3AudioTrack>(any);
                        break;
                    default:
                        throw new NotImplementedException("解析轨道错误");
                }
                model.TrackList.Add(resultTrack);
            }
        }

        public void Wirte(string path)
        {
            PackTracks();
            using (var file = File.Create(path))
            {
                Serializer.Serialize(file, this);
            }
        }

        private void PackTracks()
        {
            foreach (var track in TrackList)
            {
                Any resultAny;
                switch (track)
                {
                    case Xs3SingingTrack pbSingingTrack:
                        resultAny = Any.Pack(pbSingingTrack, Constants.TypeNames.SingingTrackTypeName);
                        break;
                    case Xs3AudioTrack pbAudioTrack:
                        resultAny = Any.Pack(pbAudioTrack, Constants.TypeNames.AudioTrackTypeName);
                        break;
                    default:
                        throw new NotImplementedException("类型错误");
                }
                _trackList.Add(resultAny);
            }
        }

        #endregion
    }
}
