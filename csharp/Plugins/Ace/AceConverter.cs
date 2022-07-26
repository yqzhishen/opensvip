using System;
using System.IO;
using System.Text;
using AceStdio.Core;
using AceStdio.Model;
using AceStdio.Options;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace AceStdio.Stream
{
    public class AceConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            AceProject aceProject;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                aceProject = JsonConvert.DeserializeObject<AceProject>(reader.ReadToEnd());
            }
            if (aceProject == null)
            {
                throw new InvalidDataException("Deserialized json object is null.");
            }
            if (aceProject.Content == null)
            {
                throw new InvalidDataException("Deserialized project content is null.");
            }
            return new AceDecoder
            {
                KeepAllPronunciation = options.GetValueAsBoolean("keepAllPronunciation"),
                ImportTension = options.GetValueAsBoolean("importTension", true),
                ImportEnergy = options.GetValueAsBoolean("importEnergy", true),
                EnergyCoefficient = Math.Min(1.0, Math.Max(0,
                    options.GetValueAsDouble("energyCoefficient", 0.5))),
                SampleInterval = Math.Max(0, options.GetValueAsInteger("curveSampleInterval", 5)),
                BreathNormalization = Math.Min(5.0, Math.Max(0, options.GetValueAsDouble("breathNormalization"))),
                TensionNormalization = Math.Min(5.0, Math.Max(0, options.GetValueAsDouble("tensionNormalization"))),
                EnergyNormalization = Math.Min(5.0, Math.Min(0, options.GetValueAsDouble("energyNormalization")))
            }.DecodeProject(aceProject.Content);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var aceProject = new AceProject
            {
                Content = new AceEncoder
                {
                    DefaultSinger = options.GetValueAsString("singer", "小夜"),
                    DefaultTempo = options.GetValueAsDouble("tempo", 120.0),
                    LagCompensation = Math.Max(0, options.GetValueAsInteger("lagCompensation", 30)),
                    BreathLength = Math.Max(0, options.GetValueAsInteger("breath", 600)),
                    MapStrengthTo = options.GetValueAsEnum("mapStrengthTo", StrengthMappingOption.Both),
                    SplitThreshold = 480 * Math.Max(0, options.GetValueAsInteger("splitThreshold", 8)),
                }.EncodeProject(project)
            };
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                var jsonString = JsonConvert.SerializeObject(aceProject);
                writer.Write(jsonString);
            }
        }
    }
}
