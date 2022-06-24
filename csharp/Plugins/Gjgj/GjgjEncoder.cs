using FlutyDeer.GjgjPlugin.Model;
using FlutyDeer.GjgjPlugin.Options;
using FlutyDeer.GjgjPlugin.Utils;
using OpenSvip.Library;
using OpenSvip.Model;

namespace FlutyDeer.GjgjPlugin
{
    public class GjgjEncoder
    {
        public LyricsAndPinyinOption LyricsAndPinyinOption { get; set; }

        public bool IsUseLegacyPinyin { get; set; }

        /// <summary>
        /// 指定的参数平均采样间隔。
        /// </summary>
        public int ParamSampleInterval { get; set; }

        /// <summary>
        /// 指定的歌手名称。
        /// </summary>
        public string SingerName { get; set; }

        private Project osProject;

        private GjProject gjProject;
        
        private TimeSynchronizer timeSynchronizer;

        /// <summary>
        /// 转换为歌叽歌叽工程。
        /// </summary>
        /// <param name="project">原始的OpenSvip工程。</param>
        /// <returns>转换后的歌叽歌叽工程。</returns>
        public GjProject EncodeProject(Project project)
        {
            osProject = project;
            InitUtils();
            gjProject = new GjProject
            {
                gjgjVersion = 2,
                ProjectSetting = ProjectSettingUtil.EncodeProjectSetting(),
                TempoMap = TempoMapUtil.EncodeTempoMap(osProject.SongTempoList, osProject.TimeSignatureList)
            };
            TrackListUtil.EncodeTracks(gjProject, osProject);
            return gjProject;
        }

        private void InitUtils()
        {
            TimeSignatureUtil.OsTimeSignatureList = osProject.TimeSignatureList;
            timeSynchronizer = new TimeSynchronizer(osProject.SongTempoList);
            TrackUtil.FirstBarBPM = osProject.SongTempoList[0].BPM;
            TrackUtil.FirstBarLength = TimeSignatureUtil.GetFirstBarLength();
            TrackUtil.IsUseLegacyPinyin = IsUseLegacyPinyin;
            TrackUtil.timeSync = timeSynchronizer;
            TrackUtil.SingerName = SingerName;
            TrackUtil.ParamSampleInterval = ParamSampleInterval;
            TrackUtil.LyricsAndPinyinOption = LyricsAndPinyinOption;
            PinyinAndLyricUtil.LyricsAndPinyinOption = LyricsAndPinyinOption;
            PhonemeUtil.FirstBarLength = TimeSignatureUtil.GetFirstBarLength();
            PhonemeUtil.TimeSynchronizer = timeSynchronizer;
        }
    }
}