using OpenSvip.Library;
using OpenSvip.Model;
using System;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class PhonemeUtil
    {
        public static TimeSynchronizer TimeSynchronizer { get; set; }
        public static int FirstBarLength { get; set; }
        /// <summary>
        /// 转换音素的第一根杆子。
        /// </summary>
        /// <param name="note">原始音符。</param>
        /// <returns>转换后的音素的第一根杆子。</returns>
        public static double GetNotePhonePreTime(Note note)
        {
            double phonePreTime = 0.0;
            try
            {
                if (note.EditedPhones != null)
                {
                    if (note.EditedPhones.HeadLengthInSecs != -1.0)
                    {
                        int noteStartPositionInTicks = note.StartPos + FirstBarLength;
                        double noteStartPositionInSeconds = TimeSynchronizer.GetActualSecsFromTicks(noteStartPositionInTicks);
                        double phoneHeadPositionInSeconds = noteStartPositionInSeconds - note.EditedPhones.HeadLengthInSecs;
                        double phoneHeadPositionInTicks = TimeSynchronizer.GetActualTicksFromSecs(phoneHeadPositionInSeconds);
                        double difference = noteStartPositionInTicks - phoneHeadPositionInTicks;
                        phonePreTime = -difference * (2000.0 / 3.0) / 480.0;
                    }
                }
            }
            catch (Exception)
            {

            }
            return phonePreTime;
        }

        /// <summary>
        /// 转换音素的第二根杆子。
        /// </summary>
        /// <param name="note">原始音符。</param>
        /// <returns>转换后的音素的第二根杆子。</returns>
        public static double GetNotePhonePostTime(Note note)
        {
            double phonePostTime = 0.0;
            try
            {
                if (note.EditedPhones != null)
                {
                    if (note.EditedPhones.MidRatioOverTail != -1.0)
                    {
                        double noteLength = note.Length;
                        double ratio = note.EditedPhones.MidRatioOverTail;
                        phonePostTime = -(noteLength / (1.0 + ratio)) * (2000.0 / 3.0) / 480.0;
                    }
                }
            }
            catch (Exception)
            {

            }
            return phonePostTime;
        }
    }
}