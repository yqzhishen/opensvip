using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Model;
using OpenSvip.Framework;
using SynthV.Core;
using SynthV.Model;
using SynthV.Options;

namespace SynthV.Stream
{
    public class SynthVConverter : IProjectConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var svProject = JsonConvert.DeserializeObject<SVProject>(reader.ReadToEnd());
            if (svProject == null)
            {
                throw new InvalidDataException("Deserialized json object is null.");
            }
            stream.Close();
            reader.Close();
            return new SynthVDecoder
            {
                PitchOption = options.GetValueAsEnum("pitch", PitchOptions.Plain),
                ImportInstantPitch = options.GetValueAsBoolean("instant", true) && svProject.InstantModeEnabled,
                BreathOption = options.GetValueAsEnum("breath", BreathOptions.Convert),
                GroupOption = options.GetValueAsEnum("group", GroupOptions.Split)
            }.DecodeProject(svProject);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var svProject = new SynthVEncoder
            {
                VibratoOption = options.GetValueAsEnum("vibrato", VibratoOptions.None),
                ParamSampleInterval = options.GetValueAsInteger("desample", 40)
            }.EncodeProject(project);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var jsonString = JsonConvert.SerializeObject(svProject);
            foreach (var ch in jsonString)
            {
                if (ch > 32 && ch < 127)
                {
                    writer.Write(ch);
                }
                else
                {
                    writer.Write($@"\u{(int) ch:x4}");
                }
            }
            writer.Flush();
            stream.WriteByte(0);
            stream.Flush();
            writer.Close();
            stream.Close();
        }
    }
}
