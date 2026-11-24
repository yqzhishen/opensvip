using OpenSvip.Model;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OxygenDioxide.UstPlugin.Stream
{
    static class UstDecoder
    {
        static readonly Encoding ShiftJIS = Encoding.GetEncoding("shift_jis");

        public static Encoding DetectEncoding(string filePath)
        {
            using (var reader = new StreamReader(filePath, ShiftJIS))
            {
                for (var i = 0; i < 10; i++)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    line = line.Trim();
                    if (line.StartsWith("Charset="))
                    {
                        return Encoding.GetEncoding(line.Replace("Charset=", ""));
                    }
                }
            }
            return ShiftJIS;
        }

        static bool ParseFloat(string s, out float value)
        {
            if (string.IsNullOrEmpty(s))
            {
                value = 0;
                return true;
            }
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public static Project DecodeFile(string filePath)
        {
            var encoding = DetectEncoding(filePath);
            using (var reader = new StreamReader(filePath, encoding))
            {
                var blocks = Ini.ReadBlocks(reader, filePath, @"\[#\w+\]");
                var lastNotePos = 0;
                var lastNoteEnd = 0;
                var project = new Project();
                var track = new SingingTrack();
                project.TrackList.Add(track);
                project.SongTempoList.Add(new SongTempo()
                {
                    Position = 0,
                    BPM = 120
                });
                //parse settings
                var settingsBlock = blocks.FirstOrDefault(b => b.header == "[#SETTING]");
                if (settingsBlock != null)
                {
                    var lines = settingsBlock.lines;
                    foreach (var iniLine in lines)
                    {
                        var line = iniLine.line;
                        var parts = line.Split(new char[] { '=' }, 2);
                        if (parts.Length != 2)
                        {
                            continue;
                        }
                        var param = parts[0].Trim();
                        switch (param)
                        {
                            case "Tempo":
                                if (ParseFloat(parts[1], out var temp))
                                {
                                    project.SongTempoList[0].BPM = temp;
                                }
                                break;
                        }
                    }
                }
                //Parse notes
                foreach (var block in blocks)
                {
                    var header = block.header;
                    switch (header)
                    {
                        case "[#VERSION]":
                            break;
                        case "[#SETTING]":
                            // Already processed
                            break;
                        case "[#TRACKEND]":
                            break;
                        default:
                            if (int.TryParse(header.Substring(2, header.Length - 3), out var noteIndex))
                            {
                                var note = new Note();
                                var ustNote = new UstNote();
                                ustNote.Parse(lastNotePos, lastNoteEnd, block.lines, out float? noteTempo);
                                note.Lyric = ustNote.lyric;
                                note.StartPos = ustNote.position;
                                note.Length = ustNote.duration;
                                note.KeyNumber = ustNote.noteNum;
                                lastNotePos = note.StartPos;
                                lastNoteEnd = note.StartPos + note.Length;
                                if (note.Lyric.ToLowerInvariant() != "r")
                                {
                                    track.NoteList.Add(note);
                                }
                                if (noteTempo != null)
                                {
                                    project.SongTempoList.Add(new SongTempo()
                                    {
                                        BPM = (float)noteTempo,
                                        Position = note.StartPos,
                                    });
                                }

                            }
                            else
                            {
                                throw new Exception($"Unexpected header\n{block.header}");
                            }
                            break;
                    }
                    
                }
                return project;
            }
        }
    }
}