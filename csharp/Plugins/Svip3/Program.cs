using System;
using System.IO;
using Xstudio.Proto;

namespace FlutyDeer.Svip3Plugin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Read();
            //Write();

            //SingAppModel singAppModel;
            //using (var input = File.OpenRead(@"D:\测试\九张机\test.dat"))
            //{
            //    singAppModel = SingAppModel.Parser.ParseFrom(input);
            //}
            //Console.WriteLine(singAppModel.ProjectFilePath);
            //Console.ReadLine();
        }

        private static void Write()
        {
            //var model = new SingAppModel
            //{
            //    Tempo = new PbTempo
            //    {
            //        BPM = 120.0f
            //    }
            //};
            //var audioPattern = new PbAudioPattern
            //{
            //    AudioFilePath = "Hello"
            //};
            //var trackContent = new PbTrackContent();
            //trackContent.PatternList.Add(audioPattern);
            //model.Tracks.Add(new PbTrack
            //{
            //    TrackContent = trackContent
            //});
            //using (var file = File.Create(@"D:\测试\test.bin"))
            //{
            //    Serializer.Serialize(file, model);
            //}
        }

        private static void Read()
        {
            AppModel appModel;
            string path = @"D:\测试\九张机\九张机.svip3";

            using (var input = File.OpenRead(path))
            {
                appModel = AppModel.Parser.ParseFrom(input);
            }
            var tracks = appModel.TrackList;
            var firstTrack = tracks[0];
            Console.WriteLine(firstTrack.TypeUrl);
            var singingTrack = firstTrack.Unpack<SingingTrack>();
            var patternList = singingTrack.PatternList;
            var pattern = patternList[0];
            var notes = pattern.NoteList;
            foreach(var note in notes)
            {
                Console.Write(note.Lyric);
            }

            //using (var file = File.OpenRead(path))
            //{
            //    singAppModel = Serializer.Deserialize<SingAppModel>(file);
            //}
            //Console.WriteLine(singAppModel.ProjectFilePath);
            //Console.WriteLine(singAppModel.Tracks[0].ClassName);

            //var track = singAppModel.Tracks[0];
            //var singingTrack = track;
            //var pattern = singingTrack.TrackContent.PatternList[0] as PbSingingPattern;
            //foreach(var note in pattern.NoteList)
            //{
            //    Console.Write(note.Lyric);
            //}
            Console.ReadLine();
        }
    }
}
