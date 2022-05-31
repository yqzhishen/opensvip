using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace FlutyDeer.MidiPlugin
{
    public static class NoteMergeUtil
    {
        public static void MergeNotes(MidiFile midiFile)
        {
            foreach (var trackChunk in midiFile.GetTrackChunks())
            {
                MergeNotes(trackChunk);
            }
        }

        private static void MergeNotes(TrackChunk trackChunk)
        {
            using (var notesManager = trackChunk.ManageNotes())
            {
                var notes = notesManager.Notes;

                // Create dictionary for storing currently merging notes of each channel (key)
                // and each pitch (key of dictionary used as value for channel)
                var currentNotes = new Dictionary<FourBitNumber, Dictionary<SevenBitNumber, Note>>();

                foreach (var note in notes.ToList())
                {
                    var channel = note.Channel;

                    // Get currently merging notes of the channel
                    if (!currentNotes.TryGetValue(channel, out var currentNotesByNoteNumber))
                        currentNotes.Add(channel, currentNotesByNoteNumber =
                                                      new Dictionary<SevenBitNumber, Note>());

                    // Get the currently merging note
                    if (!currentNotesByNoteNumber.TryGetValue(note.NoteNumber, out var currentNote))
                    {
                        currentNotesByNoteNumber.Add(note.NoteNumber, currentNote = note);
                        continue;
                    }

                    var currentEndTime = currentNote.Time + currentNote.Length;

                    // If time of the note is less than end of currently merging one,
                    // we should update length of currently merging note and delete the
                    // note from the notes collection
                    if (note.Time <= currentEndTime)
                    {
                        var endTime = Math.Max(note.Time + note.Length, currentEndTime);
                        currentNote.Length = endTime - currentNote.Time;

                        notes.Remove(note);
                    }

                    // If the note doesn't overlap currently merging one, the note become
                    // a currently merging note
                    else
                        currentNotesByNoteNumber[note.NoteNumber] = note;
                }
            }
        }
    }
}