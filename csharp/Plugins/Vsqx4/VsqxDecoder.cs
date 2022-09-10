using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Collections.Generic;
using OpenSvip.Model;

namespace Plugin.VSQX
{
    public class VsqxDecoder
    {
        public const string vsq3NameSpace = @"http://www.yamaha.co.jp/vocaloid/schema/vsq3/";
        public const string vsq4NameSpace = @"http://www.yamaha.co.jp/vocaloid/schema/vsq4/";

        public static Project Load(string file)
        {
            XmlDocument vsqx = new XmlDocument();
            vsqx.Load(file);

            XmlNamespaceManager nsmanager = new XmlNamespaceManager(vsqx.NameTable);
            nsmanager.AddNamespace("v3", vsq3NameSpace);
            nsmanager.AddNamespace("v4", vsq4NameSpace);

            XmlNode root;
            string nsPrefix;

            // Detect vsqx version
            if ((root = vsqx.SelectSingleNode("v3:vsq3", nsmanager)) != null)
            {
                nsPrefix = "v3:";
            }
            else if ((root = vsqx.SelectSingleNode("v4:vsq4", nsmanager)) != null)
            {
                nsPrefix = "v4:";
            }
            else
            {
                throw new Exception("Unrecognizable VSQx file format.");
            }

            Project project = new Project();

            string bpmPath = $"{nsPrefix}masterTrack/{nsPrefix}tempo";
            string timeSigPath = $"{nsPrefix}masterTrack/{nsPrefix}timeSig";
            string premeasurePath = $"{nsPrefix}masterTrack/{nsPrefix}preMeasure";
            string resolutionPath = $"{nsPrefix}masterTrack/{nsPrefix}resolution";
            string projectnamePath = $"{nsPrefix}masterTrack/{nsPrefix}seqName";
            string projectcommentPath = $"{nsPrefix}masterTrack/{nsPrefix}comment";
            string vsTrack = $"{nsPrefix}vsTrack";
            string monoTrack = $"{nsPrefix}monoTrack";
            string stTrack = $"{nsPrefix}stTrack";
            string tracknamePath = $"{nsPrefix}{(nsPrefix == "v3:" ? "trackName" : "name")}";
            string trackcommentPath = $"{nsPrefix}comment";
            string tracknoPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "vsTrackNo" : "tNo")}";
            string partPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "musicalPart" : "vsPart")}";
            string partnamePath = $"{nsPrefix}{(nsPrefix == "v3:" ? "partName" : "name")}";
            string partcommentPath = $"{nsPrefix}comment";
            string notePath = $"{nsPrefix}note";
            string postickPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "posTick" : "t")}";
            string durtickPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "durTick" : "dur")}";
            string notenumPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "noteNum" : "n")}";
            string velocityPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "velocity" : "v")}";
            string lyricPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "lyric" : "y")}";
            string phonemePath = $"{nsPrefix}{(nsPrefix == "v3:" ? "phnms" : "p")}";
            string playtimePath = $"{nsPrefix}playTime";
            string partstyleattrPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "partStyle" : "pStyle")}/{nsPrefix}{(nsPrefix == "v3:" ? "attr" : "v")}";
            string notestyleattrPath = $"{nsPrefix}{(nsPrefix == "v3:" ? "noteStyle" : "nStyle")}/{nsPrefix}{(nsPrefix == "v3:" ? "attr" : "v")}";
            string mixerPath = $"{nsPrefix}mixer";
            string mixerTrackPath = $"{mixerPath}/{nsPrefix}vsUnit";

            foreach (XmlNode node in root.SelectNodes(timeSigPath, nsmanager))
            {
                project.TimeSignatureList.Add(new TimeSignature
                {
                    BarIndex = Convert.ToInt32(node[nsPrefix == "v3:" ? "posMes" : "m"].InnerText),
                    Numerator = Convert.ToInt32(node[nsPrefix == "v3:" ? "nume" : "nu"].InnerText),
                    Denominator = Convert.ToInt32(node[nsPrefix == "v3:" ? "denomi" : "de"].InnerText),
                });
            }

            project.TimeSignatureList.Sort((lhs, rhs) => lhs.BarIndex.CompareTo(rhs.BarIndex));
            project.TimeSignatureList[0].BarIndex = 0;

            foreach (XmlNode node in root.SelectNodes(bpmPath, nsmanager))
            {
                project.SongTempoList.Add(new SongTempo
                {
                    Position = Convert.ToInt32(node[nsPrefix == "v3:" ? "posTick" : "t"].InnerText),
                    BPM = Convert.ToSingle(node[nsPrefix == "v3:" ? "bpm" : "v"].InnerText) / 100f,
                });
            }

            project.SongTempoList.Sort((lhs, rhs) => lhs.Position.CompareTo(rhs.Position));
            project.SongTempoList[0].Position = 0;

            var resolution = int.Parse(root.SelectSingleNode(resolutionPath, nsmanager).InnerText);
            var FilePath = file;
            var name = root.SelectSingleNode(projectnamePath, nsmanager).InnerText;
            var comment = root.SelectSingleNode(projectcommentPath, nsmanager).InnerText;

            int preMeasure = int.Parse(root.SelectSingleNode(premeasurePath, nsmanager).InnerText);
            int partPosTickShift = -preMeasure * resolution * project.TimeSignatureList[0].Numerator * 4 / project.TimeSignatureList[0].Denominator;

            #region Mixer

            var mixer = new Dictionary<int, (double volume, double pan)>();

            foreach (XmlNode trackMixer in root.SelectNodes(mixerTrackPath, nsmanager))
            {
                var tNo = int.Parse(trackMixer.SelectSingleNode($"{nsPrefix}tNo", nsmanager).InnerText);
                var iGin = double.Parse(trackMixer.SelectSingleNode($"{nsPrefix}iGin", nsmanager).InnerText);
                var pan = double.Parse(trackMixer.SelectSingleNode($"{nsPrefix}pan", nsmanager).InnerText);
                var vol = double.Parse(trackMixer.SelectSingleNode($"{nsPrefix}vol", nsmanager).InnerText);

                mixer.Add(tNo, (iGin + vol, pan));
            }

            #endregion

            foreach (XmlNode trackNode in root.SelectNodes(vsTrack, nsmanager)) // track
            {
                var track = new SingingTrack() {AISingerName = trackNode.SelectSingleNode($"{nsPrefix}name", nsmanager).NodeType.ToString(), ReverbPreset = "干声"};
                project.TrackList.Add(track);

                var trackNo = int.Parse(trackNode.SelectSingleNode(tracknoPath, nsmanager).InnerText);
                track.Title = trackNode.SelectSingleNode(tracknamePath, nsmanager).InnerText;
                if (mixer.TryGetValue(trackNo, out var trackMixer))
                {
                    track.Volume = DecodeVolume(trackMixer.volume);
                    track.Pan = trackMixer.pan;
                }
                // utrack.Comment = track.SelectSingleNode(trackcommentPath, nsmanager).InnerText;

                foreach (XmlNode part in trackNode.SelectNodes(partPath, nsmanager)) // musical part
                {
                    // var partName = part.SelectSingleNode(partnamePath, nsmanager).InnerText;
                    // var partComment = part.SelectSingleNode(partcommentPath, nsmanager).InnerText;
                    var partPosition = int.Parse(part.SelectSingleNode(postickPath, nsmanager).InnerText);
                    // var partDuration = int.Parse(part.SelectSingleNode(playtimePath, nsmanager).InnerText);

                    foreach (XmlNode noteNode in part.SelectNodes(notePath, nsmanager))
                    {
                        var note = new Note();
                        track.NoteList.Add(note);

                        note.StartPos = int.Parse(noteNode.SelectSingleNode(postickPath, nsmanager).InnerText) + partPosition + partPosTickShift;
                        note.Length = int.Parse(noteNode.SelectSingleNode(durtickPath, nsmanager).InnerText);
                        note.KeyNumber = int.Parse(noteNode.SelectSingleNode(notenumPath, nsmanager).InnerText);
                        note.Lyric = noteNode.SelectSingleNode(lyricPath, nsmanager).InnerText;
                        if (note.Lyric == "-")
                        {
                            note.Lyric = "+";
                        }
                    }

                    var pitList = new List<Tuple<int, int>>();
                    var pbsList = new List<Tuple<int, int>>();
                    foreach (XmlNode ctrlPt in part.SelectNodes($"{nsPrefix}{(nsPrefix == "v3:" ? "mCtrl" : "cc")}", nsmanager))
                    {
                        var t = int.Parse(ctrlPt.SelectSingleNode($"{nsPrefix}{(nsPrefix == "v3:" ? "posTick" : "t")}", nsmanager).InnerText);
                        t += partPosition;
                        var valNode = ctrlPt.SelectSingleNode($"{nsPrefix}{(nsPrefix == "v3:" ? "attr" : "v")}", nsmanager);
                        // type of controller
                        // D: DYN, [0,128), default: 64
                        // S: PBS, [0,24], default: 2.
                        // P: PIT, [-8192,8192), default: 0
                        // Pitch curve is calculated by multiplying PIT with PBS, max/min PIT shifts pitch by {PBS} semitones.
                        var type = valNode.Attributes["id"].Value;
                        var v = int.Parse(valNode.InnerText);
                        if (type == "DYN" || type == "D")
                        {
                            v -= 64;
                            v = (int) (v < 0 ? v / 64.0 * 240 : v / 63.0 * 120);
                            track.EditedParams.Volume.PointList.Add(new Tuple<int, int>(t, v));
                        }
                        else if (type == "PBS" || type == "S")
                        {
                            pbsList.Add(new Tuple<int, int>(t, v));
                        }
                        else if (type == "PIT" || type == "P")
                        {
                            pitList.Add(new Tuple<int, int>(t, v));
                        }
                    }

                    // Make sure that points are ordered by time
                    const int pbsDefaultVal = 2;
                    //pbsList.Sort((tuple1, tuple2) => tuple1.Item1.CompareTo(tuple2.Item1));
                    pitList.Sort((tuple1, tuple2) => tuple1.Item1.CompareTo(tuple2.Item1));

                    for (var i = 0; i < pitList.Count; i++)
                    {
                        var pt = pitList[i];
                        var t = pt.Item1;
                        var v = pt.Item2 < 0 ? pt.Item2 / 8192f : pt.Item2 / 8191f;
                        var semitone = pbsList.FindLast(tuple => tuple.Item1 <= t)?.Item2 ?? pbsDefaultVal;
                        var pit = (int) Math.Round(v * semitone * 100);
                        pit += (track.NoteList.Find(x => x.StartPos <= t && x.StartPos + x.Length > t)?.KeyNumber ?? 0) * 100;

                        pitList[i] = new Tuple<int, int>(t, pit);
                    }

                    track.EditedParams.Pitch.PointList = pitList;
                }
            }

            return project;
        }
        
        private static double DecodeVolume(double gain)
        {
            return gain >= 0
                ? Math.Min(gain / (20 * Math.Log10(2)) + 1.0, 2.0)
                : Math.Pow(10, gain / 20.0);
        }
    }
}