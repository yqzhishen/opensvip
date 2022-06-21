using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ToolGood.Words.Pinyin;

namespace OpenSvip.Library
{
    /// <summary>
    /// 用于处理汉语拼音的工具类。
    /// </summary>
    public static class PinyinUtils
    {
        /// <summary>
        /// 从汉字（歌词）序列获取拼音序列。建议在需要将歌词批量转换为拼音时使用此方法，因为将单字组成词语后可获得更准确的转换结果。
        /// </summary>
        /// <param name="chineseSeries">需要转换的汉语歌词序列。</param>
        /// <param name="ignoreHyphens">是否忽略连音符号“-”。设置为 true 时，连音符号不会分开前后的汉字，从而避免语句被切断；设置为 false 时，连音符将被视为一个非汉字符号，参与对语句的分割。</param>
        /// <param name="reserveLetters">是否保留输入中的英文字母（可能原本就是拼音）。</param>
        /// <param name="filterNonChinese">是否过滤除英文字母和连字符外的非汉字符号。设置为 true 时，输出将仅保留由汉字转换而来的拼音；设置为 false 时，非汉字字符将原样原位保留在输出中。</param>
        /// <returns>一个拼音序列，其元素个数保证与输入序列相等。</returns>
        public static string[] GetPinyinSeries(
            IEnumerable<string> chineseSeries,
            bool ignoreHyphens = true,
            bool reserveLetters = true,
            bool filterNonChinese = true)
        {
            var chineseArray = chineseSeries.ToArray();
            var pinyinArray = new string[chineseArray.Length];
            var itemCountArray = new int[chineseArray.Length];
            var resultItems = new List<string>();
            var chineseBuilder = new StringBuilder();
            for (var i = 0; i < chineseArray.Length; ++i)
            {
                var count = 0;
                var isLetter = false;
                var nonChineseBuilder = new StringBuilder();
                foreach (var c in chineseArray[i])
                {
                    if (char.IsWhiteSpace(c) || ignoreHyphens && c == '-')
                    {
                        if (nonChineseBuilder.Length > 0)
                        {
                            resultItems.Add(nonChineseBuilder.ToString());
                            ++count;
                            nonChineseBuilder.Clear();
                        }
                    }
                    else if (WordsHelper.IsAllChinese(c.ToString()))
                    {
                        if (nonChineseBuilder.Length > 0)
                        {
                            resultItems.Add(nonChineseBuilder.ToString());
                            ++count;
                            nonChineseBuilder.Clear();
                        }
                        chineseBuilder.Append(c);
                        ++count;
                    }
                    else
                    {
                        if (chineseBuilder.Length > 0)
                        {
                            resultItems.AddRange(WordsHelper.GetPinyin(chineseBuilder.ToString(), " ").ToLower().Split());
                            chineseBuilder.Clear();
                        }

                        if (c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z')
                        {
                            if (reserveLetters)
                            {
                                if (nonChineseBuilder.Length > 0 && !isLetter)
                                {
                                    resultItems.Add(nonChineseBuilder.ToString());
                                    ++count;
                                    nonChineseBuilder.Clear();
                                }
                                nonChineseBuilder.Append(c);
                                isLetter = true;
                            }
                        }
                        else if (!filterNonChinese)
                        {
                            if (nonChineseBuilder.Length > 0 && isLetter)
                            {
                                resultItems.Add(nonChineseBuilder.ToString());
                                ++count;
                                nonChineseBuilder.Clear();
                            }
                            nonChineseBuilder.Append(c);
                            isLetter = false;
                        }
                    }
                }

                if (nonChineseBuilder.Length > 0)
                {
                    resultItems.Add(nonChineseBuilder.ToString());
                    ++count;
                    nonChineseBuilder.Clear();
                }
                itemCountArray[i] = count;
            }

            if (chineseBuilder.Length > 0)
            {
                resultItems.AddRange(WordsHelper.GetPinyin(chineseBuilder.ToString(), " ").ToLower().Split());
            }

            var index = 0;
            var pinyinBuilder = new StringBuilder();
            for (var i = 0; i < pinyinArray.Length; ++i)
            {
                if (itemCountArray[i] == 0)
                {
                    pinyinArray[i] = "";
                    continue;
                }

                pinyinBuilder.Append(resultItems[index++]);
                for (var j = 1; j < itemCountArray[i]; ++j)
                {
                    if (Regex.IsMatch(resultItems[index - 1].Last().ToString(), "[0-9A-Za-z]")
                        && Regex.IsMatch(resultItems[index].First().ToString(), "[0-9A-Za-z]"))
                    {
                        pinyinBuilder.Append(' ');
                    }
                    pinyinBuilder.Append(resultItems[index++]);
                }

                pinyinArray[i] = pinyinBuilder.ToString();
                pinyinBuilder.Clear();
            }

            return pinyinArray;
        }
    }
}
