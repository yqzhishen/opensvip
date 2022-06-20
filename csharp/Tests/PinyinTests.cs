using System;
using System.Linq;
using NPinyin;
using NUnit.Framework;
using ToolGood.Words.Pinyin;

namespace OpenSvip.Tests
{
    [TestFixture]
    public class PinyinTests
    {
        [Test]
        public void TestPolyphonic01()
        {
            var hanzi = new[]
            {
                "强",
                "降",
                "长",
                "行",
                "传",
                "重"
            };
            var pinyin = new[]
            {
                "qiang",
                "jiang",
                "chang",
                "hang",
                "chuan",
                "zhong"
            };
            foreach (var pair in hanzi.Zip(pinyin, (s1, s2) => new Tuple<string, string>(s1, s2)))
            {
                var res = WordsHelper.GetPinyin(pair.Item1, " ").ToLower();
                Assert.AreEqual(pair.Item2, res);
            }
        }
        
        [Test]
        public void TestPolyphonic02()
        {
            var hanzi = new[]
            {
                "行业",
                "行走",
                "还是",
                "还原",
                "表率",
                "概率",
                "传递",
                "传记"
            };
            var pinyin = new[]
            {
                "hang ye",
                "xing zou",
                "hai shi",
                "huan yuan",
                "biao shuai",
                "gai lv",
                "chuan di",
                "zhuan ji"
            };
            foreach (var pair in hanzi.Zip(pinyin, (s1, s2) => new Tuple<string, string>(s1, s2)))
            {
                var res = WordsHelper.GetPinyin(pair.Item1, " ").ToLower();
                Assert.AreEqual(pair.Item2, res);
            }
        }
        
        [Test]
        public void TestSpecialVowel()
        {
            var hanzi = new[]
            {
                "女儿",
                "铝材",
                "举办",
                "虐待"
            };
            var pinyin = new[]
            {
                "nv er",
                "lv cai",
                "ju ban",
                "nve dai"
            };
            foreach (var pair in hanzi.Zip(pinyin, (s1, s2) => new Tuple<string, string>(s1, s2)))
            {
                var res = WordsHelper.GetPinyin(pair.Item1, " ").ToLower();
                Assert.AreEqual(pair.Item2, res);
            }
        }
        
        [Test]
        public void TestRareWords()
        {
            var hanzi = new[]
            {
                "饕餮",
                "耄耋",
                "丨",
                "泩鼙秶",
                "柊雪"
            };
            var pinyin = new[]
            {
                "tao tie",
                "mao die",
                "gun",
                "sheng pi zi",
                "zhong xue"
            };
            foreach (var pair in hanzi.Zip(pinyin, (s1, s2) => new Tuple<string, string>(s1, s2)))
            {
                var res = WordsHelper.GetPinyin(pair.Item1, " ").ToLower();
                Assert.AreEqual(pair.Item2, res);
            }
        }

        [Test]
        public void TestNonChinese()
        {
            var hanzi = new[]
            {
                "English",
                "pin yin",
                "   ",
                "\r\n\t",
                "，。！",
                "牛b啊兄弟666",
                "连音-符号",
            };
            var pinyin = new[]
            {
                "e n g l i s h",
                "p i n   y i n",
                "     ",
                "\r \n \t",
                "， 。 ！",
                "niu b a xiong di 6 6 6",
                "lian yin - fu hao",
            };
            foreach (var pair in hanzi.Zip(pinyin, (s1, s2) => new Tuple<string, string>(s1, s2)))
            {
                var res = WordsHelper.GetPinyin(pair.Item1, " ").ToLower();
                Assert.AreEqual(pair.Item2, res);
            }
        }
    }
}
