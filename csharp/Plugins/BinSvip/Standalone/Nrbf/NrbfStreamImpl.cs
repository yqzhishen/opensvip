using System;
using System.Runtime.InteropServices;
using System.Text;
using BinSvip.Standalone.Model;

namespace BinSvip.Standalone.Nrbf
{
    internal unsafe class NrbfStreamImpl
    {
        public string ErrorMessage;

        public NrbfStream.StatusType Status;

        public NrbfStreamImpl()
        {
            ErrorMessage = "";
            Status = NrbfStream.StatusType.Ok;
        }

        private void copy_string(byte[] src, NrbfLibrary.xs_string* dst)
        {
            int size = src == null ? 0 : src.Length;
            dst->size = size;
            if (size == 0)
            {
                dst->str = null;
            }
            else
            {
                // Allocate string
                dst->str = (byte*)NrbfLibrary.qnrbf_malloc(src.Length);

                // Copy data
                var gch = GCHandle.Alloc(src, GCHandleType.Pinned);
                NrbfLibrary.qnrbf_memcpy(dst->str, (byte*)Marshal.UnsafeAddrOfPinnedArrayElement(src, 0), src.Length);
                gch.Free();
            }
        }

        private void copy_string(string src, NrbfLibrary.xs_string* dst)
        {
            if (String.IsNullOrEmpty(src))
            {
                copy_string(Array.Empty<byte>(), dst);
                return;
            }

            copy_string(Encoding.GetEncoding("UTF-8").GetBytes(src), dst);
        }

        private byte[] from_xs_string_bytes(NrbfLibrary.xs_string str)
        {
            if (str.size == 0)
            {
                return Array.Empty<byte>();
            }
            
            byte[] bytes = new byte[str.size];
            {
                var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                NrbfLibrary.qnrbf_memcpy((void*)Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0), str.str, str.size);
                gch.Free();
            }
            return bytes;
        }

        private string from_xs_string(NrbfLibrary.xs_string str)
        {
            return Encoding.GetEncoding("UTF-8").GetString(from_xs_string_bytes(str));
        }

        private LineParam createLineParam(NrbfLibrary.xs_node* lineParams)
        {
            var res = new LineParam();

            var node = lineParams;
            while (node != null)
            {
                var inParam = (NrbfLibrary.xs_line_param*)node->data;

                // Save one
                res.nodeLinkedList.AddLast(new LineParamNode(inParam->Pos, inParam->Value));

                node = node->next;
            }

            return res;
        }

        private NrbfLibrary.xs_node* create_line_param_list(LineParam lineParam)
        {
            NrbfLibrary.xs_node* head = null;
            var p = head;

            foreach (var item in lineParam.nodeLinkedList)
            {
                var param =
                    (NrbfLibrary.xs_line_param*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_line_param));
                param->Pos = item.Pos;
                param->Value = item.Value;

                // Create node
                var node = (NrbfLibrary.xs_node*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_node));
                node->data = param;
                node->next = null;

                if (p != null)
                {
                    p->next = node;
                }
                else
                {
                    head = node;
                }

                p = node;
            }

            return head;
        }

        public AppModel Read(byte[] data)
        {
            // Allocate context
            var ctx = NrbfLibrary.qnrbf_xstudio_alloc_context();

            // Copy data to buffer
            copy_string(data, &ctx->buf);

            // This call will clean all data in context, no need to free buffer
            NrbfLibrary.qnrbf_xstudio_read(ctx);

            // Check status and copy result
            AppModel model = null;
            Status = (NrbfStream.StatusType)ctx->status;
            if (Status != NrbfStream.StatusType.Ok)
            {
                ErrorMessage = from_xs_string(ctx->error);
            }
            else
            {
                var inModel = ctx->model;

                // Copy model data
                model = new AppModel();

                model.ProjectFilePath = from_xs_string(inModel->ProjectFilePath);
                model.quantize = inModel->quantize;
                model.isTriplet = inModel->isTriplet;
                model.isNumericalKeyName = inModel->isNumericalKeyName;
                model.firstNumericalKeyNameAtIndex = inModel->firstNumericalKeyNameAtIndex;

                // Copy tempo list
                {
                    var node = inModel->tempoList;
                    while (node != null)
                    {
                        var inTempo = (NrbfLibrary.xs_song_tempo*)node->data;

                        var tempo = new SongTempo();
                        tempo.Overlapped = inTempo->@base.Overlapped;
                        tempo.pos = inTempo->pos;
                        tempo.tempo = inTempo->tempo;

                        // Save one
                        model.tempoList.Add(tempo);

                        node = node->next;
                    }
                }

                // Copy beat list
                {
                    var node = inModel->beatList;
                    while (node != null)
                    {
                        var inBeat = (NrbfLibrary.xs_song_beat*)node->data;

                        var beat = new SongBeat();
                        beat.Overlapped = inBeat->@base.Overlapped;
                        beat.barIndex = inBeat->barIndex;
                        beat.beatSize = new BeatSize(inBeat->beatSize.x, inBeat->beatSize.y);

                        // Save one
                        model.beatList.Add(beat);

                        node = node->next;
                    }
                }

                // Copy track list
                {
                    var node = inModel->trackList;
                    while (node != null)
                    {
                        ITrack track;
                        var baseTrack = (NrbfLibrary.xs_track*)node->data;

                        if (baseTrack->track_type == NrbfLibrary.xs_track_type.SINGING)
                        {
                            var inTrack = (NrbfLibrary.xs_singing_track*)baseTrack;
                            var singingTrack = new SingingTrack();
                            singingTrack.AISingerId = from_xs_string(inTrack->AISingerId);
                            singingTrack.needRefreshBaseMetadataFlag = inTrack->needRefreshBaseMetadataFlag;
                            singingTrack.reverbPreset = (ReverbPreset)inTrack->reverbPreset;

                            // Copy params
                            if (inTrack->editedPitchLine != null)
                                singingTrack.editedPitchLine = createLineParam(inTrack->editedPitchLine);

                            if (inTrack->editedVolumeLine != null)
                                singingTrack.editedVolumeLine = createLineParam(inTrack->editedVolumeLine);

                            if (inTrack->editedBreathLine != null)
                                singingTrack.editedBreathLine = createLineParam(inTrack->editedBreathLine);

                            if (inTrack->editedGenderLine != null)
                                singingTrack.editedGenderLine = createLineParam(inTrack->editedGenderLine);

                            if (inTrack->editedPowerLine != null)
                                singingTrack.editedPowerLine = createLineParam(inTrack->editedPowerLine);

                            // Copy notes
                            {
                                var noteNode = inTrack->noteList;
                                while (noteNode != null)
                                {
                                    var inNote = (NrbfLibrary.xs_note*)noteNode->data;

                                    var note = new Note();
                                    note.Overlapped = inNote->@base.Overlapped;
                                    note.VibratoPercent = inNote->VibratoPercent;
                                    note.startPos = inNote->startPos;
                                    note.widthPos = inNote->widthPos;
                                    note.keyIndex = inNote->keyIndex;
                                    note.lyric = from_xs_string(inNote->lyric);
                                    note.pronouncing = from_xs_string(inNote->pronouncing);
                                    note.headTag = (NoteHeadTag)inNote->headTag;

                                    // Copy NotePhoneInfo
                                    if (inNote->NotePhoneInfo != null)
                                    {
                                        var org = inNote->NotePhoneInfo;

                                        // Save one
                                        note.NotePhoneInfo = new NotePhoneInfo(
                                            org->HeadPhoneTimeInSec,
                                            org->MidPartOverTailPartRatio);
                                    }

                                    // Copy Vibrato
                                    if (inNote->Vibrato != null)
                                    {
                                        var org = inNote->Vibrato;

                                        var vibrato = new VibratoStyle();
                                        vibrato.IsAntiPhase = org->IsAntiPhase;
                                        if (org->ampLine != null)
                                            vibrato.ampLine = createLineParam(org->ampLine);
                                        if (org->freqLine != null)
                                            vibrato.freqLine = createLineParam(org->freqLine);

                                        // Save one
                                        note.Vibrato = vibrato;
                                    }

                                    // Copy VibratoPercentInfo
                                    if (inNote->VibratoPercentInfo != null)
                                    {
                                        var org = inNote->VibratoPercentInfo;

                                        // Save one
                                        note.VibratoPercentInfo = new VibratoPercentInfo(
                                            org->startPercent,
                                            org->endPercent);
                                    }

                                    // Save one
                                    singingTrack.noteList.Add(note);

                                    noteNode = noteNode->next;
                                }
                            }

                            // Save one
                            track = singingTrack;
                        }
                        else
                        {
                            var inTrack = (NrbfLibrary.xs_instrument_track*)baseTrack;
                            var instrumentTrack = new InstrumentTrack();

                            instrumentTrack.SampleRate = inTrack->SampleRate;
                            instrumentTrack.SampleCount = inTrack->SampleCount;
                            instrumentTrack.ChannelCount = inTrack->ChannelCount;
                            instrumentTrack.OffsetInPos = inTrack->OffsetInPos;
                            instrumentTrack.InstrumentFilePath = from_xs_string(inTrack->InstrumentFilePath);

                            // Save one
                            track = instrumentTrack;
                        }

                        // Copy track base
                        track.volume = baseTrack->volume;
                        track.pan = baseTrack->pan;
                        track.name = from_xs_string(baseTrack->name);
                        track.mute = baseTrack->mute;
                        track.solo = baseTrack->solo;

                        // Save one
                        model.trackList.Add(track);

                        node = node->next;
                    }
                }
            }

            // Free context
            NrbfLibrary.qnrbf_xstudio_free_context(ctx);

            return model;
        }

        public byte[] Write(AppModel model)
        {
            // Set output data
            var resModel = (NrbfLibrary.xs_app_model*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_app_model));

            copy_string(model.ProjectFilePath, &resModel->ProjectFilePath);
            resModel->quantize = model.quantize;
            resModel->isTriplet = model.isTriplet;
            resModel->isNumericalKeyName = model.isNumericalKeyName;
            resModel->firstNumericalKeyNameAtIndex = model.firstNumericalKeyNameAtIndex;

            // Copy tempo list
            {
                ref var head = ref resModel->tempoList;
                head = null;
                var p = head;
                foreach (var item in model.tempoList)
                {
                    var tempo = (NrbfLibrary.xs_song_tempo*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_song_tempo));
                    tempo->@base.Overlapped = item.Overlapped;
                    tempo->pos = item.pos;
                    tempo->tempo = item.tempo;

                    var node = (NrbfLibrary.xs_node*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_node));
                    node->data = tempo;
                    node->next = null;

                    // Create node
                    if (p != null)
                    {
                        p->next = node;
                    }
                    else
                    {
                        head = node;
                    }

                    p = node;
                }
            }

            // Copy beat list
            {
                ref var head = ref resModel->beatList;
                head = null;
                var p = head;
                foreach (var item in model.beatList)
                {
                    var beat = (NrbfLibrary.xs_song_beat*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_song_beat));
                    beat->@base.Overlapped = item.Overlapped;
                    beat->barIndex = item.barIndex;
                    beat->beatSize.x = item.beatSize.x;
                    beat->beatSize.y = item.beatSize.y;

                    var node = (NrbfLibrary.xs_node*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_node));
                    node->data = beat;
                    node->next = null;

                    // Create node
                    if (p != null)
                    {
                        p->next = node;
                    }
                    else
                    {
                        head = node;
                    }

                    p = node;
                }
            }

            // Copy track list
            {
                ref var head = ref resModel->trackList;
                head = null;
                var p = head;
                foreach (var item in model.trackList)
                {
                    NrbfLibrary.xs_track* trackPtr;

                    if (item is SingingTrack singingTrack)
                    {
                        var track =
                            (NrbfLibrary.xs_singing_track*)NrbfLibrary.qnrbf_malloc(
                                sizeof(NrbfLibrary.xs_singing_track));
                        track->@base.track_type = NrbfLibrary.xs_track_type.SINGING;

                        copy_string(singingTrack.AISingerId, &track->AISingerId);
                        track->needRefreshBaseMetadataFlag = singingTrack.needRefreshBaseMetadataFlag;
                        track->reverbPreset = (int)singingTrack.reverbPreset;

                        // Copy track params
                        track->editedPitchLine =
                            singingTrack.editedPitchLine == null
                                ? null
                                : create_line_param_list(singingTrack.editedPitchLine);
                        track->editedVolumeLine =
                            singingTrack.editedVolumeLine == null
                                ? null
                                : create_line_param_list(singingTrack.editedVolumeLine);
                        track->editedBreathLine =
                            singingTrack.editedBreathLine == null
                                ? null
                                : create_line_param_list(singingTrack.editedBreathLine);
                        track->editedGenderLine =
                            singingTrack.editedGenderLine == null
                                ? null
                                : create_line_param_list(singingTrack.editedGenderLine);
                        track->editedPowerLine =
                            singingTrack.editedPowerLine == null
                                ? null
                                : create_line_param_list(singingTrack.editedPowerLine);

                        // Copy note list
                        {
                            ref var head2 = ref track->noteList;
                            head2 = null;
                            var p2 = head2;
                            foreach (var noteItem in singingTrack.noteList)
                            {
                                var note = (NrbfLibrary.xs_note*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_note));
                                note->@base.Overlapped = noteItem.Overlapped;
                                note->VibratoPercent = noteItem.VibratoPercent;
                                note->startPos = noteItem.startPos;
                                note->widthPos = noteItem.widthPos;
                                note->keyIndex = noteItem.keyIndex;
                                copy_string(noteItem.lyric, &note->lyric);
                                copy_string(noteItem.pronouncing, &note->pronouncing);
                                note->headTag = (NrbfLibrary.xs_note_head_tag)noteItem.headTag;

                                // Copy NotePhoneInfo
                                if (noteItem.NotePhoneInfo != null)
                                {
                                    var org = noteItem.NotePhoneInfo;
                                    var info =
                                        (NrbfLibrary.xs_note_phone_info*)NrbfLibrary.qnrbf_malloc(
                                            sizeof(NrbfLibrary.xs_note_phone_info));

                                    info->HeadPhoneTimeInSec = org.HeadPhoneTimeInSec;
                                    info->MidPartOverTailPartRatio = org.MidPartOverTailPartRatio;

                                    // Save one
                                    note->NotePhoneInfo = info;
                                }
                                else
                                {
                                    note->NotePhoneInfo = null;
                                }

                                // Copy VibratoStyle
                                if (noteItem.Vibrato != null)
                                {
                                    var org = noteItem.Vibrato;
                                    var vibrato =
                                        (NrbfLibrary.xs_vibrato_style*)NrbfLibrary.qnrbf_malloc(
                                            sizeof(NrbfLibrary.xs_vibrato_style));

                                    vibrato->IsAntiPhase = org.IsAntiPhase;
                                    vibrato->ampLine =
                                        org.ampLine == null
                                            ? null
                                            : create_line_param_list(org.ampLine);
                                    vibrato->freqLine =
                                        org.freqLine == null
                                            ? null
                                            : create_line_param_list(org.freqLine);

                                    // Save one
                                    note->Vibrato = vibrato;
                                }
                                else
                                {
                                    note->Vibrato = null;
                                }

                                // Copy VibratoPercentInfo
                                if (noteItem.VibratoPercentInfo != null)
                                {
                                    var org = noteItem.VibratoPercentInfo;
                                    var info = (NrbfLibrary.xs_vibrato_percent_info*)NrbfLibrary.qnrbf_malloc(
                                        sizeof(NrbfLibrary.xs_vibrato_percent_info));

                                    info->startPercent = org.startPercent;
                                    info->endPercent = org.endPercent;

                                    // Save one
                                    note->VibratoPercentInfo = info;
                                }
                                else
                                {
                                    note->VibratoPercentInfo = null;
                                }

                                // Create node
                                var node2 = (NrbfLibrary.xs_node*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_node));
                                node2->data = note;
                                node2->next = null;

                                if (p2 != null)
                                {
                                    p2->next = node2;
                                }
                                else
                                {
                                    head2 = node2;
                                }

                                p2 = node2;
                            }
                        }

                        // Save one
                        trackPtr = (NrbfLibrary.xs_track*)track;
                    }
                    else
                    {
                        var instrumentTrack = (InstrumentTrack)item;
                        var track =
                            (NrbfLibrary.xs_instrument_track*)NrbfLibrary.qnrbf_malloc(
                                sizeof(NrbfLibrary.xs_instrument_track));
                        track->@base.track_type = NrbfLibrary.xs_track_type.INSTRUMENT;

                        track->SampleRate = instrumentTrack.SampleRate;
                        track->SampleCount = instrumentTrack.SampleCount;
                        track->ChannelCount = instrumentTrack.ChannelCount;
                        track->OffsetInPos = instrumentTrack.OffsetInPos;
                        copy_string(instrumentTrack.InstrumentFilePath, &track->InstrumentFilePath);

                        // Save one
                        trackPtr = (NrbfLibrary.xs_track*)track;
                    }

                    // Copy base
                    trackPtr->volume = item.volume;
                    trackPtr->pan = item.pan;
                    trackPtr->mute = item.mute;
                    trackPtr->solo = item.solo;
                    copy_string(item.name, &trackPtr->name);

                    // Create node
                    var node = (NrbfLibrary.xs_node*)NrbfLibrary.qnrbf_malloc(sizeof(NrbfLibrary.xs_node));
                    node->data = trackPtr;
                    node->next = null;

                    if (p != null)
                    {
                        p->next = node;
                    }
                    else
                    {
                        head = node;
                    }

                    p = node;
                }
            }

            // Allocate context
            var ctx = NrbfLibrary.qnrbf_xstudio_alloc_context();

            // Set input model
            ctx->model = resModel;

            // This call will clean all data in context, no need to free buffer
            NrbfLibrary.qnrbf_xstudio_write(ctx);

            // Check status and copy result
            byte[] bytes = null;
            Status = (NrbfStream.StatusType)ctx->status;
            if (Status != NrbfStream.StatusType.Ok)
            {
                ErrorMessage = from_xs_string(ctx->error);
            }
            else
            {
                bytes = from_xs_string_bytes(ctx->buf);
            }

            // Free context
            NrbfLibrary.qnrbf_xstudio_free_context(ctx);

            return bytes;
        }
    }
}