using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using OpenSvip.Framework;
using OpenSvip.Model;
using XSAppModel.NrbfFormat;

namespace OpenSvip.Stream.Standalone
{
    public class NrbfSvipConverter
    {
        public Project Load(string path, ConverterOptions options)
        {
            return DoLoad(path);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            DoSave(path, project, options);
        }

        private Project DoLoad(string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(stream);
            var version = reader.ReadString();
            var versionNumber = reader.ReadString();

            version += versionNumber; // Unused version

            XSAppModel.XStudio.AppModel model;

            // Call native library
            {
                var ns = new NrbfStream();
                model = ns.Read(reader.ReadBytes((int)(stream.Length - stream.Position)));

                // Call dispose to free library
                ns.Dispose();

                if (model == null)
                {
                    throw new FileLoadException(ns.ErrorMessage);
                }
            }
            reader.Close();
            stream.Close();
            return new NrbfSvipDecoder().DecodeProject(version, model);
        }

        private void DoSave(string path, Project project, ConverterOptions options)
        {
            var (version, model) = new NrbfSvipEncoder()
            {
                DefaultSinger = options.GetValueAsString("singer", "陈水若"),
                DefaultTempo = options.GetValueAsInteger("tempo", 60)
            }.EncodeProject(project);
            var verEnum = options.GetValueAsEnum<BinarySvipVersions>("version");
            switch (verEnum)
            {
                case BinarySvipVersions.SVIP7_0_0:
                    version = "SVIP7.0.0";
                    break;
                case BinarySvipVersions.Automatic:
                    if (version != "SVIP0.0.0")
                    {
                        break;
                    }

                    goto case BinarySvipVersions.SVIP6_0_0;
                case BinarySvipVersions.SVIP6_0_0:
                    version = "SVIP6.0.0";
                    break;
                case BinarySvipVersions.Compatible:
                    version = "SVIP0.0.0";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var index = Regex.Match(version, @"\d+\.\d+\.\d+").Groups[0].Index;
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new BinaryWriter(stream);
            writer.Write(version.Substring(0, index));
            writer.Write(version.Substring(index, version.Length - index));
            writer.Flush();

            // Call native library
            {
                // Write binary svip to byte array
                var ns = new NrbfStream();
                var bytes = ns.Write(model);

                // Call dispose to free library
                ns.Dispose();

                if (ns.Status != NrbfStream.StatusType.Ok)
                {
                    throw new FileLoadException(ns.ErrorMessage);
                }

                writer.Write(bytes);
                writer.Flush();
            }

            writer.Close();
            stream.Close();
        }
    }
}