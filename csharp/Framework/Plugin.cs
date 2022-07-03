using System;
using System.Xml.Serialization;

namespace OpenSvip.Framework
{
    [Serializable]
    [XmlRoot("Plugin")]
    public class Plugin
    {
        [XmlElement] public string Name { get; set; }
        
        [XmlElement] public string Version { get; set; }

        [XmlElement] public string Author { get; set; }
        
        [XmlElement] public string HomePage { get; set; }

        [XmlIgnore] private string _updateUri;

        [XmlElement]
        public string UpdateUri
        {
            get => _updateUri ?? (_updateUri == "none" ? null : ConstValues.PluginUpdateRoot + $"plugin-{Identifier}.toml");
            set => _updateUri = value;
        }
        
        [XmlElement] public string Descriptions { get; set; }
        
        [XmlElement] public string TargetFramework { get; set; } = "1.0.0";

        [XmlElement] public string Requirements { get; set; }
        
        [XmlElement] public string Format { get; set; }
        
        [XmlElement] public string Suffix { get; set; }
        
        [XmlElement] public string Identifier { get; set; }
        
        [XmlElement] public string LibraryPath { get; set; }
        
        [XmlElement] public string Converter { get; set; }
        
        [XmlArray] public Option[] InputOptions { get; set; } = Array.Empty<Option>();
        
        [XmlArray] public Option[] OutputOptions { get; set; } = Array.Empty<Option>();
    }
}
