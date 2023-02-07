using FlutyDeer.Svip3Plugin.Proto;
using OpenSvip.Model;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// X Studio 3 工程文件
    /// </summary>
    [ProtoContract]
    public class Svip3Model
    {
        #region Properties & Fields

        /// <summary>
        /// 工程文件路径
        /// </summary>
        [ProtoMember(1)]
        public string ProjectFilePath { get; set; }

        /// <summary>
        /// 版本
        /// </summary>
        [ProtoMember(2)]
        public string Version { get; set; }

        /// <summary>
        /// 歌曲时长
        /// </summary>
        [ProtoMember(3)]
        public int Duration { get; set; } = 192000;

        /// <summary>
        /// 曲速列表
        /// </summary>
        [ProtoMember(4)]
        public List<Xs3Tempo> TempoList { get; set; } = new List<Xs3Tempo>();

        /// <summary>
        /// 拍号列表
        /// </summary>
        [ProtoMember(5)]
        public List<Xs3TimeSignature> TimeSignatureList { get; set; } = new List<Xs3TimeSignature>();

        /// <summary>
        /// 轨道列表（Any类型）
        /// </summary>
        [ProtoMember(6)]
        private readonly List<Any> _trackList = new List<Any>();

        /// <summary>
        /// 轨道列表
        /// </summary>
        public List<object> TrackList { get; set; } = new List<object>();

        /// <summary>
        /// 总线
        /// </summary>
        [ProtoMember(7)]
        public Xs3Master Master { get; set; }

        /// <summary>
        /// 调性
        /// </summary>
        [ProtoMember(8)]
        public string Tonality { get; set; } = "C大调";

        /// <summary>
        /// 量化值（音符粒度）
        /// </summary>
        [ProtoMember(9)]
        public int QuantizeValue { get; set; } = 16;

        /// <summary>
        /// 循环开始位置
        /// </summary>
        [ProtoMember(10)]
        public int LoopStartPos { get; set; } = 0;

        /// <summary>
        /// 循环结束位置
        /// </summary>
        [ProtoMember(11)]
        public int LoopEndPos { get; set; } = 1920;

        /// <summary>
        /// 是否开启量化
        /// </summary>
        [ProtoMember(12)]
        public bool IsQuantize { get; set; } = true;

        #endregion

        #region Methods

        /// <summary>
        /// 读取指定路径的 svip3 工程文件。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 解包轨道。
        /// </summary>
        /// <param name="model"></param>
        /// <exception cref="NotImplementedException"></exception>
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

        /// <summary>
        /// 写入工程文件到指定路径。
        /// </summary>
        /// <param name="path"></param>
        public void Write(string path)
        {
            PackTracks();
            using (var file = File.Create(path))
            {
                Serializer.Serialize(file, this);
            }
        }

        /// <summary>
        /// 打包轨道。
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void PackTracks()
        {
            if (TrackList.Any())
            foreach (var track in TrackList)
            {
                Any resultAny = new Any();
                switch (track)
                {
                    case Xs3SingingTrack singingTrack:
                        if (singingTrack == null)
                            break;
                        resultAny = Any.Pack(singingTrack, Constants.TypeNames.SingingTrack);
                        break;
                    case Xs3AudioTrack audioTrack:
                        if (audioTrack == null)
                            break;
                        resultAny = Any.Pack(audioTrack, Constants.TypeNames.AudioTrack);
                        break;
                    default:
                        throw new NotImplementedException("类型错误");
                }
                if (resultAny.Value != null)
                _trackList.Add(resultAny);
            }
        }

        #endregion
    }
}
