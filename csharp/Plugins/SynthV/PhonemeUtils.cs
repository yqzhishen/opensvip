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

        private static readonly Dictionary<string, string> XsampaDictionary;

        private static readonly Dictionary<string, double> DefaultDurations = new Dictionary<string, double>
        {
            {"stop", 0.10},
            {"affricate", 0.125},
            {"fricative", 0.125},
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
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    "PhonemeDict.json"),
                FileMode.Open,
                FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.Default);
            PhonemeDictionary = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            stream = new FileStream(
                Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    "XsampaDict.json"),
                FileMode.Open,
                FileAccess.Read);
            reader = new StreamReader(stream, Encoding.Default);
            XsampaDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
        }
        public static List<string> LyricsToPinyin(List<string> lyrics)
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

        public static string XsampaToPinyin(string xsampa)
        {
            xsampa = Regex.Replace(xsampa, @"\s+", " ").Trim(' ');
            return !XsampaDictionary.ContainsKey(xsampa) ? "la" : XsampaDictionary[xsampa];
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