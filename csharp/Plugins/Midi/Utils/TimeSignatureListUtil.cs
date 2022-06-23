using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeSignature = OpenSvip.Model.TimeSignature;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class TimeSignatureListUtil
    {
        public static List<TimeSignature> DecodeTimeSignatureList(TempoMap tempoMap, bool IsImportTimeSignatures)
        {
            List<TimeSignature> timeSignatureList = new List<TimeSignature>();
            var changes = tempoMap.GetTimeSignatureChanges();
            if (changes != null && changes.Count() > 0)
            {
                var firstTimeSignatureTime = changes.First().Time;
                if (firstTimeSignatureTime != 0)//由于位于0且为四四拍的拍号不存在，所以需要添加一个拍号
                {
                    timeSignatureList.Add(new TimeSignature
                    {
                        BarIndex = 0,
                        Numerator = 4,
                        Denominator = 4
                    });
                }
                if (IsImportTimeSignatures)
                {
                    foreach (var change in changes)
                    {
                        var time = change.Time;
                        var timeSignature = change.Value;
                        MetricTimeSpan metricTime = TimeConverter.ConvertTo<MetricTimeSpan>(time, tempoMap);
                        BarBeatTicksTimeSpan barBeatTicksTimeFromMetric = TimeConverter.ConvertTo<BarBeatTicksTimeSpan>(metricTime, tempoMap);
                        timeSignatureList.Add(new TimeSignature
                        {
                            BarIndex = (int)barBeatTicksTimeFromMetric.Bars,
                            Numerator = timeSignature.Numerator,
                            Denominator = timeSignature.Denominator
                        });
                    }
                }
            }
            else
            {
                timeSignatureList.Add(new TimeSignature
                {
                    BarIndex = 0,
                    Numerator = 4,
                    Denominator = 4
                });
            }
            return timeSignatureList;
        }
    }
}
