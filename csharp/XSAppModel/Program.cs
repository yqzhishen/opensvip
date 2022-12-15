// See https://aka.ms/new-console-template for more information

using System;
using System.IO;
using System.Text;
using XSAppModel;
using XSAppModel.NrbfFormat;
using XSAppModel.XStudio;

public static class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine(
                $"Usage: {Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().ProcessName)} <svip file> [output file]");
            return 0;
        }

        var filename = args[0];
        var model = ReadSvip(filename);
        if (model == null)
        {
            return -1;
        }

        // Correct line params
        foreach (var track in model.trackList)
        {
            if (track is SingingTrack singingTrack)
            {
                if (singingTrack.editedPitchLine == null)
                {
                    singingTrack.editedPitchLine = new LineParam();
                    singingTrack.editedPitchLine.setDefault();
                }

                if (singingTrack.editedVolumeLine == null)
                {
                    singingTrack.editedVolumeLine = new LineParam();
                    singingTrack.editedVolumeLine.setDefault();
                }

                if (singingTrack.editedBreathLine == null)
                {
                    singingTrack.editedBreathLine = new LineParam();
                    singingTrack.editedBreathLine.setDefault();
                }

                if (singingTrack.editedGenderLine == null)
                {
                    singingTrack.editedGenderLine = new LineParam();
                    singingTrack.editedGenderLine.setDefault();
                }

                if (singingTrack.editedPowerLine == null)
                {
                    singingTrack.editedPowerLine = new LineParam();
                    singingTrack.editedPowerLine.setDefault();
                }
            }
        }

        // Print some information
        Console.WriteLine($"ProjectFilePath: {model.ProjectFilePath}");
        Console.WriteLine($"quantize: {model.quantize}");
        Console.WriteLine($"isTriplet: {model.isTriplet}");
        Console.WriteLine($"isNumericalKeyName: {model.isNumericalKeyName}");
        Console.WriteLine($"firstNumericalKeyNameAtIndex: {model.firstNumericalKeyNameAtIndex}");

        Console.WriteLine($"tempoList: {model.tempoList.Count}");
        foreach (var item in model.tempoList)
        {
            Console.WriteLine($"  pos:{item.pos}, tempo:{item.tempo}");
        }

        Console.WriteLine($"SongBeat: {model.beatList.Count}");
        foreach (var item in model.beatList)
        {
            Console.WriteLine($"  index:{item.barIndex}, x:{item.beatSize.x}, y:{item.beatSize.y}");
        }

        Console.WriteLine($"trackList: {model.trackList.Count}");
        foreach (var item in model.trackList)
        {
            if (item is SingingTrack singingTrack)
            {
                Console.WriteLine($"  SingingTrack");
                Console.WriteLine($"    AISingerId: {singingTrack.AISingerId}");
                Console.WriteLine($"    noteLIst: {singingTrack.noteList.Count}");
                Console.WriteLine($"    reverbPreset: {singingTrack.reverbPreset}");
            }
            else
            {
                var instrumentTrack = (InstrumentTrack)item;
                Console.WriteLine($"  InstrumentTrack");
                Console.WriteLine($"    SampleRate: {instrumentTrack.SampleRate}");
                Console.WriteLine($"    SampleCount: {instrumentTrack.SampleCount}");
                Console.WriteLine($"    ChannelCount: {instrumentTrack.ChannelCount}");
                Console.WriteLine($"    OffsetInPos: {instrumentTrack.OffsetInPos}");
                Console.WriteLine($"    InstrumentFilePath: {instrumentTrack.InstrumentFilePath}");
            }
        }

        Console.WriteLine("Successfully read binary to model.");

        // Write json file
        // var outputFilename = args.Length < 2 ? Path.GetFileNameWithoutExtension(filename) + ".json" : args[1];
        // File.WriteAllBytes(outputFilename, Encoding.GetEncoding("UTF-8").GetBytes(Serializer.Serialize(model)));
        // Console.WriteLine("Successfully convert binary svip to json.");

        // Write svip back
        var outputFilename =
            args.Length < 2 ? Path.GetFileNameWithoutExtension(filename) + "_back_dotnet.svip" : args[1];
        if (!WriteSvip(outputFilename, model))
        {
            return -1;
        }

        Console.WriteLine("Successfully write model to binary.");
        return 0;
    }

    static AppModel ReadSvip(string filename)
    {
        // Check existence
        if (!File.Exists(filename))
        {
            Console.WriteLine($"File \"{filename}\" not exists.");
            return null;
        }

        // Read file
        byte[] svipData;
        {
            // Read
            var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            var reader = new BinaryReader(fs);

            // Read version
            var version = reader.ReadString();
            var versionNumber = reader.ReadString();

            // Read binary data to byte array
            svipData = reader.ReadBytes((int)(fs.Length - fs.Position));

            Console.WriteLine($"Version: {version} {versionNumber}");
        }

        var stream = new NrbfStream();
                       var model = stream.Read(svipData);
        if (model == null) // equivalent to `if (stream.Status != NrbfStream.StatusType.Ok)`
        {
            Console.WriteLine(stream.ErrorMessage);
        }

        // Call dispose to free library
        stream.Dispose();
        
        return model;
    }

    static bool WriteSvip(string filename, AppModel model)
    {
        // Write binary svip to byte array
        var stream = new NrbfStream();
        var bytes = stream.Write(model);

        bool success = true;
        if (stream.Status != NrbfStream.StatusType.Ok)
        {
            Console.WriteLine(stream.ErrorMessage);
            success = false;
        }
        else
        {
            // Write to file
            var fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var writer = new BinaryWriter(fs);
            writer.Write("SVIP");
            writer.Write("6.0.0");
            writer.Write(bytes);
            writer.Flush();
        }

        // Call dispose to free library
        stream.Dispose();

        return success;
    }
}