using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;

namespace Plugin.Vogen
{
    public static class ZipHelper
    {
        //压缩文件
        public static void CompressFile(string filePath, string zipPath)
        {
            using (ZipOutputStream s = new ZipOutputStream(File.Create(zipPath)))
            {
                s.SetLevel(9);
                byte[] buffer = new byte[4096];
                ZipEntry entry = new ZipEntry(Path.GetFileName(filePath));
                entry.DateTime = DateTime.Now;
                s.PutNextEntry(entry);
                using (FileStream fs = File.OpenRead(filePath))
                {
                    int sourceBytes;
                    do
                    {
                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                        s.Write(buffer, 0, sourceBytes);
                    } while (sourceBytes > 0);
                }
                s.Finish();
                s.Close();
            }
        }

        //解压文件
        public static void DecompressFile(string sourceFile, string targetPath)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format("未能找到文件 '{0}' ", sourceFile));
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            using (var s = new ZipInputStream(File.OpenRead(sourceFile)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if (theEntry.IsDirectory)
                    {
                        continue;
                    }
                    string directorName = Path.Combine(targetPath, Path.GetDirectoryName(theEntry.Name));
                    string fileName = Path.Combine(directorName, Path.GetFileName(theEntry.Name));
                    if (!Directory.Exists(directorName))
                    {
                        Directory.CreateDirectory(directorName);
                    }
                    if (!String.IsNullOrEmpty(fileName))
                    {
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            int size = 4096;
                            byte[] data = new byte[size];
                            while (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                                size = s.Read(data, 0, data.Length);
                            }
                        }
                    }
                }
            }
        }
    }
}
