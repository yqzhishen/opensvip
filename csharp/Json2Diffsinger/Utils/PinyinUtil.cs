using OpenSvip.Library;
using System;
using System.Collections.Generic;

namespace Json2DiffSinger.Utils
{
    /// <summary>
    /// 用于处理拼音的工具。含声母韵母拆分，汉字转拼音。
    /// </summary>
    public static class PinyinUtil
    {
        public static readonly string[] PureVovels = { "a", "o", "e" };

        public static readonly string[] SingleInitials = { "b", "p", "m", "f", "d", "t", "n", "l",
            "g", "k", "h", "j", "q", "x", "r", "z", "c", "s", "y", "w" };

        public static readonly string[] DoubleInitials = { "zh", "ch", "sh" };
        
        public static readonly string[] JqxyInitials = { "j", "q", "x", "y" };

        private static List<string> pinyinList = new List<string>();
        
        /// <summary>
        /// 将拼音拆分成声母和韵母。
        /// </summary>
        /// <param name="pinyin">拼音。</param>
        /// <returns></returns>
        public static Tuple<string, string> Split(string pinyin)
        {
            //修复v的错误
            if (IsStartWithJqxy(pinyin))
            {
                if (pinyin[1] == 'u')
                {
                    pinyin = pinyin.Replace('u', 'v');
                }
            }
            if (HasDoubleInitial(pinyin))//有声母zh- ch- sh-
            {
                var initial = pinyin.Substring(0, 2);
                var rest = pinyin.Substring(2);
                return new Tuple<string, string>(initial, rest);
            }
            else if (HasSingleInitial(pinyin))//有其他声母
            {
                var initial = pinyin.Substring(0, 1);
                var rest = pinyin.Substring(1);
                return new Tuple<string, string>(initial, rest);
            }
            else//没有声母
            {
                return new Tuple<string, string>("", pinyin);
            }
        }

        private static bool IsStartWithJqxy(string pinyin)
        {
            bool flag = false;
            foreach (var jqxInitial in JqxyInitials)
            {
                if (pinyin.StartsWith(jqxInitial))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        private static bool HasSingleInitial(string pinyin)
        {
            bool flag = false;
            foreach (var initial in SingleInitials)
            {
                if (pinyin.StartsWith(initial))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        private static bool HasDoubleInitial(string pinyin)
        {
            bool flag = false;
            foreach (var initial in DoubleInitials)
            {
                if (pinyin.StartsWith(initial))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 从歌词添加拼音。
        /// </summary>
        /// <param name="lyricList">歌词。</param>
        public static void AddPinyinFromLyrics(List<string> lyricList)
        {
            var pinyinArray = PinyinUtils.GetPinyinSeries(lyricList);
            foreach (var pinyin in pinyinArray)
            {
                pinyinList.Add(pinyin);
            }
        }

        /// <summary>
        /// 清空所有拼音。
        /// </summary>
        public static void ClearAllPinyin()
        {
            pinyinList.Clear();
        }

        /// <summary>
        /// 获取音符的拼音。
        /// </summary>
        /// <param name="noteLyric"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetNotePinyin(string noteLyric, int index)
        {
            string pinyin;
            if (noteLyric.Contains("-"))
            {
                return "-";
            }
            else
            {
                pinyin = pinyinList[index];
                return pinyin;
            }
        }
    }
}
