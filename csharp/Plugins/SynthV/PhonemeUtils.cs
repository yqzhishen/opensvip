using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NPinyin;

namespace Plugin.SynthV
{
    public static class PhonemeUtils
    {
        private static readonly Dictionary<string, string[]> PhonemeDictionary;

        private static readonly Dictionary<string, double> DefaultDurations = new Dictionary<string, double>
        {
            {"stop", 0.110},
            {"affricate", 0.120},
            {"fricative", 0.120},
            {"aspirate", 0.094},
            {"liquid", 0.062},
            {"nasal", 0.094},
            {"vowel", 0.0},
            {"semivowel", 0.055},
            {"diphthong", 0.0}
        };

        private const double DefaultPhoneRatio = 1.8;

        static PhonemeUtils()
        {
            var stream = new FileStream(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\PhonemeDict.json",
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.Default);
            PhonemeDictionary = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
        }
        public static List<string> LyricsToPinyin(IEnumerable<string> lyrics)
        {
            var pinyinList = new List<string>();
            var builder = new StringBuilder();
            foreach (var lyric in lyrics)
            {
                if (Regex.IsMatch(lyric, "[a-zA-Z]"))
                {
                    if (builder.Length > 0)
                    {
                        pinyinList.AddRange(Regex.Split(Pinyin.GetPinyin(builder.ToString()).Trim(' '), "\\s+"));
                        builder.Clear();
                    }
                    pinyinList.Add(lyric);
                }
                else
                {
                    builder.Append(lyric);
                }
            }
            if (builder.Length > 0)
            {
                pinyinList.AddRange(Regex.Split(Pinyin.GetPinyin(builder.ToString()).Trim(' '), "\\s+"));
            }
            return pinyinList;
        }

        public static int NumberOfPhones(string pinyin)
        {
            if (!PhonemeDictionary.ContainsKey(pinyin))
            {
                pinyin = "la";
            }
            return PhonemeDictionary[pinyin].Length;
        }

        public static double[] DefaultPhoneMarks(string pinyin)
        {
            var res = new double[2];
            if (pinyin == "-")
            {
                return res;
            }
            if (!PhonemeDictionary.ContainsKey(pinyin))
            {
                pinyin = "la";
            }
            var phoneParts = PhonemeDictionary[pinyin];
            res[0] = DefaultDurations[phoneParts[0]];
            var index = phoneParts[0] == "vowel" || phoneParts[0] == "diphthong" ? 0 : 1;
            res[1] = index < phoneParts.Length ? DefaultPhoneRatio : 0.0;
            return res;
        }
    }
}