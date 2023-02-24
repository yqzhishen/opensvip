using System;
using Json2DiffSinger.Core.Models;
using Json2DiffSinger.Options;
using Json2DiffSinger.Utils;
using OpenSvip.Library;
using OpenSvip.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Json2DiffSinger
{
    public class DiffSingerEncoder
    {
        public string Dictionary { get; set; }

        /// <summary>
        /// 音素参数模式选项
        /// </summary>
        public PhonemeModeOption PhonemeOption { get; set; }

        /// <summary>
        /// 音高参数模式选项
        /// </summary>
        public PitchModeOption PitchModeOption { get; set; }

        /// <summary>
        /// 性别参数选项
        /// </summary>
        public bool IsExportGender { get; set; }

        /// <summary>
        /// 转为 ds 参数
        /// </summary>
        /// <param name="project"></param>
        /// <param name="trailingSpace"></param>
        /// <returns></returns>
        public AbstractParamsModel Encode(Project project, float trailingSpace = 0.05f)
        {
            var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var dictPath = Path.Combine(currentPath, "Dictionaries", $"{Dictionary}.txt");
            PinyinUtil.LoadPhonemeTable(dictPath);
            TimeSynchronizer synchronizer = new TimeSynchronizer(project.SongTempoList);
            SingingTrack singingTrack = project.TrackList
                .OfType<SingingTrack>()
                .First();
            List<Note> osNotes = singingTrack.NoteList;
            var dsProject = new DsProject
            {
                NoteList = NoteListUtils.Encode(osNotes, synchronizer, trailingSpace)
            };
            var totalDuration = (int)Math.Round(dsProject.NoteList.Sum(note => note.Duration) * 1000);
            if (PitchModeOption == PitchModeOption.Manual)
            {
                var osPitchParamCurve = singingTrack.EditedParams.Pitch;
                dsProject.PitchParamCurve = PitchParamUtils.Encode(osPitchParamCurve, totalDuration);
            }

            if (IsExportGender)
            {
                var osGenderParamCurve = singingTrack.EditedParams.Gender;
                dsProject.GenderParamCurve = GenderParamUtils.Encode(osGenderParamCurve, totalDuration);
            }

            var model = DsProject.ToParamModel(dsProject);
            if (PhonemeOption == PhonemeModeOption.Auto)
            {
                model.PhonemeDurationSequence = null;
            }

            return model;
        }
    }
}
