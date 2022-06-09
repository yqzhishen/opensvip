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
    }
}
