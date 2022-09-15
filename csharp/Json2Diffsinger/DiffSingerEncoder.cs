using Json2DiffSinger.Core.Models;
using Json2DiffSinger.Options;
using Json2DiffSinger.Utils;
using OpenSvip.Library;
using OpenSvip.Model;
using System.Collections.Generic;
using System.Linq;

namespace Json2DiffSinger
{
    public class DiffSingerEncoder
    {
        /// <summary>
        /// 音素参数模式选项
        /// </summary>
        public PhonemeModeOption PhonemeOption {get;set;}

        /// <summary>
        /// 音高参数模式选项
        /// </summary>
        public PitchModeOption PitchModeOption {get;set;}

        /// <summary>
        /// 转为 ds 参数
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public AbstractParamsModel Encode(Project project)
        {
            TimeSynchronizer synchronizer = new TimeSynchronizer(project.SongTempoList);
            SingingTrack singingTrack = project.TrackList
                .OfType<SingingTrack>()
                .First();
            List<Note> osNotes = singingTrack.NoteList;
            var dsProject = new DsProject
            {
                NoteList = NoteListUtils.Encode(osNotes, synchronizer)
            };
            if (PitchModeOption == PitchModeOption.Manual)
            {
                var osPitchParamCuvre = singingTrack.EditedParams.Pitch;
                dsProject.PitchParamCurve = PitchParamUtils.Encode(osPitchParamCuvre);
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
