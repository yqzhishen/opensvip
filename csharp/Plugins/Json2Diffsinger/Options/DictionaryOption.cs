using System;
using System.ComponentModel;

namespace Json2DiffSinger.Options
{
    public enum DictionaryOption
    {
        [Description("opencpop")]
        [DictionaryPath("opencpop.txt")]
        Opencpop,
        [Description("opencpop-strict")]
        [DictionaryPath("opencpop-strict.txt")]
        OpencpopStrict,
        [Description("opencpop-extension")]
        [DictionaryPath("opencpop-extension.txt")]
        OpencpopExtension
    }

    public class DictionaryPathAttribute : Attribute
    {
        public string Filename { get; private set; }
        
        public DictionaryPathAttribute(string filename)
        {
            Filename = filename;
        }
    }
}
