using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecodeMidi.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ReadingSettings readingSettings = new ReadingSettings();
            readingSettings.TextEncoding = Encoding.UTF8;
            var midiFile = MidiFile.Read(@"D:\Users\fluty\Downloads\bpmtest.mid", readingSettings);
            string s = "";
            IEnumerable<TrackChunk> midiChunk = midiFile.GetTrackChunks();
            foreach (var chunkItem in midiChunk)
            {
                IEnumerable<MidiEvent> midiEvent = chunkItem.Events;
                s += chunkItem.ChunkId + "\n";
                foreach (var eventItem  in midiEvent)
                {
                    s = s + " 事件内容：" + eventItem.ToString() + " 时间差：" + eventItem.DeltaTime + "\n";
                }
            }
            var stream = new FileStream(@"D:\Users\fluty\Downloads\bpmtest.txt", FileMode.Create, FileAccess.Write);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            foreach (var ch in s)
            {
                writer.Write(ch);
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
    }
}
