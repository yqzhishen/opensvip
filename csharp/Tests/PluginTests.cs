using System.Collections.Generic;
using NUnit.Framework;
using OpenSvip.Framework;
using SynthV.Stream;

namespace OpenSvip.Tests
{
    [TestFixture]
    public class PluginTests
    {
        [Test]
        public void TestSynthVLoad01()
        {
            new SynthVConverter().Load(
                @"C:\Users\YQ之神\Desktop\【无参】破云来.svp",
                new ConverterOptions(new Dictionary<string, string>()));
        }
    }
}