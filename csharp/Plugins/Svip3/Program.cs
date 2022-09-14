using FlutyDeer.Svip3Plugin.Model;
using System;

namespace FlutyDeer.Svip3Plugin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Read();
            Write();
        }

        private static void Write()
        {
            var model = new Svip3Model();
            model.TempoList.Add(new Xs3Tempo
            {
                Tempo = 120
            });
            model.TimeSignatureList.Add(new Xs3TimeSignature
            {
                Content = new Xs3TimeSignatureContent
                {
                    Numerator = 4,
                    Denominator = 4
                }
            });
            model.TrackList.Add(new Xs3AudioTrack
            {
                Name = "测试音频轨",
                Color = "#66CCFF"
            });
            model.Wirte(@"D:\测试\test.svip3");
        }

        private static void Read()
        {
            Svip3Model model;
            string path = @"D:\测试\音量参数\音量参数.svip3";
            model = Svip3Model.Read(path);
            Console.WriteLine(model.Version);
            Console.ReadLine();
        }
    }
}
