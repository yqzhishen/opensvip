using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenSvip.Library;
using OpenSvip.Model;

namespace OpenSvip.Tests
{
    [TestFixture]
    public class LibraryTests
    {
        [Test]
        public void TestCurveSplit01()
        {
            var curve = new ParamCurve
            {
                PointList = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(1, 1),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(3, 0),
                    new Tuple<int, int>(4, 1),
                    new Tuple<int, int>(5, 1)
                }
            };
            var segments = curve.SplitIntoSegments();
            Assert.AreEqual(1, segments.Count);
            Assert.IsTrue(segments[0].SequenceEqual(new List<Tuple<int, int>>
            {
                new Tuple<int, int>(1, 1),
                new Tuple<int, int>(2, 1),
                new Tuple<int, int>(3, 0),
                new Tuple<int, int>(4, 1),
                new Tuple<int, int>(5, 1)
            }));
        }

        [Test]
        public void TestCurveSplit02()
        {
            var curve = new ParamCurve
            {
                PointList = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(1, 0),
                    new Tuple<int, int>(2, 0),
                    new Tuple<int, int>(3, 1),
                    new Tuple<int, int>(4, 1),
                    new Tuple<int, int>(5, 0),
                    new Tuple<int, int>(6, 0),
                    new Tuple<int, int>(7, 1),
                    new Tuple<int, int>(8, 1),
                    new Tuple<int, int>(9, 0),
                    new Tuple<int, int>(10, 0)
                }
            };
            var segments = curve.SplitIntoSegments();
            Assert.AreEqual(2, segments.Count);
            Assert.IsTrue(segments[0].SequenceEqual(new List<Tuple<int, int>>
            {
                new Tuple<int, int>(3, 1),
                new Tuple<int, int>(4, 1)
            }));
            Assert.IsTrue(segments[1].SequenceEqual(new List<Tuple<int, int>>
            {
                new Tuple<int, int>(7, 1),
                new Tuple<int, int>(8, 1)
            }));
        }

        [Test]
        public void TestCurveSplit03()
        {
            var curve = new ParamCurve
            {
                PointList = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(1, 0),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(3, 1),
                    new Tuple<int, int>(4, 0)
                }
            };
            var segments = curve.SplitIntoSegments();
            Assert.AreEqual(1, segments.Count);
            Assert.IsTrue(segments[0].SequenceEqual(new List<Tuple<int, int>>
            {
                new Tuple<int, int>(1, 0),
                new Tuple<int, int>(2, 1),
                new Tuple<int, int>(3, 1),
                new Tuple<int, int>(4, 0),
            }));
        }

        [Test]
        public void TestCurveSplit04()
        {
            var curve = new ParamCurve
            {
                PointList = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(-192000, 0),
                    new Tuple<int, int>(2, 1),
                    new Tuple<int, int>(3, 1),
                    new Tuple<int, int>(int.MaxValue / 2, 0)
                }
            };
            var segments = curve.SplitIntoSegments();
            Assert.AreEqual(1, segments.Count);
            Assert.IsTrue(segments[0].SequenceEqual(new List<Tuple<int, int>>
            {
                new Tuple<int, int>(2, 1),
                new Tuple<int, int>(3, 1)
            }));
        }

        private readonly string[] _chinese =
        {
            "山东菏泽",
            "曹县，",
            "牛pi",
            "666我滴",
            "宝贝儿！",
            "行-走-",
            "行-业-",
            "-"
        };

        [Test]
        public void TestPinyin01()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian",
                "niu pi",
                "wo di",
                "bao bei er",
                "xing zou",
                "hang ye",
                ""
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese);
            Assert.AreEqual(pinyin, result);
        }
        
        [Test]
        public void TestPinyin02()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian",
                "niu pi",
                "wo di",
                "bao bei er",
                "hang zou",
                "hang ye",
                ""
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese, false);
            Assert.AreEqual(pinyin, result);
        }
        
        [Test]
        public void TestPinyin03()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian",
                "niu",
                "wo di",
                "bao bei er",
                "xing zou",
                "hang ye",
                ""
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese, true, false);
            Assert.AreEqual(pinyin, result);
        }
        
        [Test]
        public void TestPinyin04()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian",
                "niu",
                "wo di",
                "bao bei er",
                "hang zou",
                "hang ye",
                ""
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese, false, false);
            Assert.AreEqual(pinyin, result);
        }

        [Test]
        public void TestPinyin05()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian，",
                "niu pi",
                "666 wo di",
                "bao bei er！",
                "xing zou",
                "hang ye",
                ""
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese, true, true, false);
            Assert.AreEqual(pinyin, result);
        }
        
        [Test]
        public void TestPinyin06()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian，",
                "niu pi",
                "666 wo di",
                "bao bei er！",
                "hang-zou-",
                "hang-ye-",
                "-"
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese, false, true, false);
            Assert.AreEqual(pinyin, result);
        }
        
        [Test]
        public void TestPinyin07()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian，",
                "niu",
                "666 wo di",
                "bao bei er！",
                "xing zou",
                "hang ye",
                ""
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese, true, false, false);
            Assert.AreEqual(pinyin, result);
        }
        
        [Test]
        public void TestPinyin08()
        {
            var pinyin = new[]
            {
                "shan dong he ze",
                "cao xian，",
                "niu",
                "666 wo di",
                "bao bei er！",
                "hang-zou-",
                "hang-ye-",
                "-"
            };
            var result = PinyinUtils.GetPinyinSeries(_chinese, false, false, false);
            Assert.AreEqual(pinyin, result);
        }
    }
}