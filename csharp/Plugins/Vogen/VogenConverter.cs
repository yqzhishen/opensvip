using System.IO;
using System.Text;
using Newtonsoft.Json;
using OpenSvip.Framework;
using OpenSvip.Model;
using System;
using Plugin.Vogen;
using Vogen.Model;
using ICSharpCode.SharpZipLib.Zip;

namespace Vogen.Stream
{
    internal class VogenConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var vogenProject = JsonConvert.DeserializeObject<VogenProject>(reader.ReadToEnd());
            stream.Close();
            reader.Close();
            return new VogenDecoder().DecodeProject(vogenProject);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            var VogenProject = new VogenEncoder().EncodeProject(project);
            var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            var jsonString = JsonConvert.SerializeObject(VogenProject);
            foreach (var ch in jsonString)
            {
                writer.Write(ch);
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
            string chartJsonPath = Path.GetDirectoryName(path) + "\\" + "chart.json";
            FileInfo fi = new FileInfo(path);
            fi.MoveTo(chartJsonPath);
            ZipFile(chartJsonPath, Path.GetDirectoryName(path) + "\\" +  Path.GetFileNameWithoutExtension(path) + ".vog");
            File.Delete(chartJsonPath);
        }

        /// <summary>
        /// 单文件压缩
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="zipedFile">zip压缩文件</param>
        /// <param name="blockSize">缓冲区大小</param>
        /// <param name="compressionLevel">压缩级别</param>
        public static void ZipFile(string sourceFile, string zipedFile, int blockSize = 1024, int compressionLevel = 6)
        {
            if (!File.Exists(sourceFile))
            {
                throw new System.IO.FileNotFoundException("The specified file " + sourceFile + " could not be found.");
            }
            var fileName = System.IO.Path.GetFileName(sourceFile);

            FileStream streamToZip = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            FileStream zipFile = File.Create(zipedFile);
            ZipOutputStream zipStream = new ZipOutputStream(zipFile);

            ZipEntry zipEntry = new ZipEntry(fileName);
            zipStream.PutNextEntry(zipEntry);

            //存储、最快、较快、标准、较好、最好  0-9
            zipStream.SetLevel(compressionLevel);

            byte[] buffer = new byte[blockSize];

            int size = streamToZip.Read(buffer, 0, buffer.Length);
            zipStream.Write(buffer, 0, size);
            try
            {
                while (size < streamToZip.Length)
                {
                    int sizeRead = streamToZip.Read(buffer, 0, buffer.Length);
                    zipStream.Write(buffer, 0, sizeRead);
                    size += sizeRead;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            zipStream.Finish();
            zipStream.Close();
            streamToZip.Close();
        }
    }
}