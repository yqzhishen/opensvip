from typing import List, Tuple, Dict

import clr
from System.Collections.Generic import List as CSharpList

clr.AddReference(r'C:\Users\YQ之神\AppData\Local\warp\packages\XStudioSinger_2.0.0_beta2.exe\SingingTool.Model.dll')
from SingingTool.Model import AppModel, ITrack, SingingTrack, InstrumentTrack, Note, NotePhoneInfo, VibratoStyle, VibratoPercentInfo
from SingingTool.Model.SingingGeneralConcept import SongTempo, SongBeat
from SingingTool.Model.Line import LineParam, LineParamNode

from const import OpenSvipSingers, OpenSvipReverbPresets, OpenSvipNoteHeadTags


class OpenSvipModel:
    def __init__(self):
        self.Version: str = ''
        self.SongTempoList: List[OpenSvipSongTempo] = []
        self.TimeSignatureList: List[OpenSvipTimeSignature] = []
        self.TrackList: List[OpenSvipTrack] = []

    def decode(self, version: str, model: AppModel):
        self.Version = version
        for tempo in model.get_TempoList():
            self.SongTempoList.append(OpenSvipSongTempo().decode(tempo))
        for beat in model.get_BeatList():
            self.TimeSignatureList.append(OpenSvipTimeSignature().decode(beat))
        for track in model.get_TrackList():
            ty = track.GetType().get_FullName()
            if ty == 'SingingTool.Model.SingingTrack':
                self.TrackList.append(OpenSvipSingingTrack().decode(track))
            elif ty == 'SingingTool.Model.InstrumentTrack':
                self.TrackList.append(OpenSvipInstrumentalTrack().decode(track))
        return self

    def dedict(self, obj: dict):
        self.Version = obj['Version']
        for ele in obj['SongTempoList']:
            self.SongTempoList.append(OpenSvipSongTempo().dedict(ele))
        for ele in obj['TimeSignatureList']:
            self.TimeSignatureList.append(OpenSvipTimeSignature().dedict(ele))
        for ele in obj['TrackList']:
            if ele['Type'] == 'Singing':
                self.TrackList.append(OpenSvipSingingTrack().dedict(ele))
            elif ele['Type'] == 'Instrumental':
                self.TrackList.append(OpenSvipInstrumentalTrack().dedict(ele))
        return self

    def encode(self) -> Tuple[str, AppModel]:
        model = AppModel()
        tempo_list = model.get_TempoList()
        for tempo in self.SongTempoList:
            tempo_list.InsertItemInOrder(tempo.encode())
        beat_list = model.get_BeatList()
        for beat in self.TimeSignatureList:
            beat_list.InsertItemInOrder(beat.encode())
        track_list = CSharpList[ITrack]()
        for track in self.TrackList:
            ele = track.encode()
            if ele is not None:
                track_list.Add(ele)
        model.set_TrackList(track_list)
        return self.Version, model


class OpenSvipSongTempo:
    def __init__(self):
        self.Position: int = 0
        self.BPM: float = 0.

    def decode(self, tempo: SongTempo):
        self.Position = tempo.get_Pos()
        self.BPM = tempo.get_Tempo() / 100.
        return self

    def dedict(self, obj: dict):
        self.Position = obj['Position']
        self.BPM = obj['BPM']
        return self

    def encode(self) -> SongTempo:
        tempo = SongTempo()
        tempo.set_Pos(self.Position)
        tempo.set_Tempo(round(self.BPM * 100))
        return tempo


class OpenSvipTimeSignature:
    def __init__(self):
        self.BarIndex: int = 0
        self.Numerator: int = 0
        self.Denominator: int = 0

    def decode(self, beat: SongBeat):
        self.BarIndex = beat.get_BarIndex()
        frac = beat.get_BeatSize()
        self.Numerator = frac.get_X()
        self.Denominator = frac.get_Y()
        return self

    def dedict(self, obj: dict):
        self.BarIndex = obj['BarIndex']
        self.Numerator = obj['Numerator']
        self.Denominator = obj['Denominator']
        return self

    def encode(self) -> SongBeat:
        beat = SongBeat()
        beat.set_BarIndex(self.BarIndex)
        val = beat.get_BeatSize()
        val.set_X(self.Numerator)
        val.set_Y(self.Denominator)
        return beat


class OpenSvipTrack:
    def __init__(self, ty):
        self.Type: str = ty
        self.Title: str = ''
        self.Mute: bool = False
        self.Solo: bool = False
        self.Volume: float = 0.
        self.Pan: float = 0.

    def decode(self, track: ITrack):
        self.Title = track.get_Name()
        self.Mute = track.get_Mute()
        self.Solo = track.get_Solo()
        self.Volume = track.get_Volume()
        self.Pan = track.get_Pan()
        return self

    def dedict(self, obj: dict):
        self.Type = obj['Type']
        self.Title = obj['Title']
        self.Mute = obj['Mute']
        self.Solo = obj['Solo']
        self.Volume = obj['Volume']
        self.Pan = obj['Pan']
        return self

    def encode(self) -> ITrack:
        if self.Type == 'Singing':
            track = SingingTrack()
        elif self.Type == 'Instrumental':
            track = InstrumentTrack()
        else:
            return None
        track.set_Name(self.Title)
        track.set_Mute(self.Mute)
        track.set_Solo(self.Solo)
        track.set_Volume(self.Volume)
        track.set_Pan(self.Pan)
        return track


class OpenSvipSingingTrack(OpenSvipTrack):
    def __init__(self):
        super().__init__('Singing')
        self.AISingerName: str = ''
        self.ReverbPreset: str = ''
        self.NoteList: List[OpenSvipNote] = []
        self.EditedParams: OpenSvipParams = OpenSvipParams()

    def decode(self, track: SingingTrack):
        super().decode(track)
        self.AISingerName = OpenSvipSingers.get_name(track.get_AISingerId())
        self.ReverbPreset = OpenSvipReverbPresets.get_name(track.get_ReverbPreset())
        for note in track.get_NoteList():
            self.NoteList.append(OpenSvipNote().decode(note))
        self.EditedParams.decode(track)
        return self

    def dedict(self, obj: dict):
        super().dedict(obj)
        self.AISingerName = obj['AISingerName']
        self.ReverbPreset = obj['ReverbPreset']
        for ele in obj['NoteList']:
            self.NoteList.append(OpenSvipNote().dedict(ele))
        self.EditedParams.dedict(obj['EditedParams'])
        return self

    def encode(self) -> SingingTrack:
        track = super().encode()
        track.set_AISingerId(OpenSvipSingers.get_id(self.AISingerName))
        track.set_ReverbPreset(OpenSvipReverbPresets.get_index(self.ReverbPreset))
        note_list = track.get_NoteList()
        for note in self.NoteList:
            note_list.InsertItemInOrder(note.encode())
        params = self.EditedParams.encode()
        track.set_EditedPitchLine(params['Pitch'])
        track.set_EditedVolumeLine(params['Volume'])
        track.set_EditedBreathLine(params['Breath'])
        track.set_EditedGenderLine(params['Gender'])
        track.set_EditedPowerLine(params['Strength'])
        return track


class OpenSvipInstrumentalTrack(OpenSvipTrack):
    def __init__(self):
        super().__init__('Instrumental')
        self.AudioFilePath: str = ''
        self.Offset: int = 0

    def decode(self, track: InstrumentTrack):
        super().decode(track)
        self.AudioFilePath = track.get_InstrumentFilePath()
        self.Offset = track.get_OffsetInPos()
        return self

    def dedict(self, obj: dict):
        super().dedict(obj)
        self.AudioFilePath = obj['AudioFilePath']
        self.Offset = obj['Offset']
        return self

    def encode(self) -> InstrumentTrack:
        track = super().encode()
        track.set_InstrumentFilePath(self.AudioFilePath)
        track.set_OffsetInPos(self.Offset)
        return track


class OpenSvipNote:
    def __init__(self):
        self.StartPos: int = 0
        self.Length: int = 0
        self.KeyNumber: int = 0
        self.HeadTag: str = None
        self.Lyric: str = ''
        self.Pronunciation: str = None
        self.EditedPhones: OpenSvipPhones = None
        self.Vibrato: OpenSvipVibrato = None

    def decode(self, note: Note):
        self.StartPos = note.get_ActualStartPos()
        self.Length = note.get_WidthPos()
        self.KeyNumber = note.get_KeyIndex() - 12
        self.HeadTag = OpenSvipNoteHeadTags.get_name(note.get_HeadTag())
        self.Lyric = note.get_Lyric()
        pronunciation = note.get_Pronouncing()
        if pronunciation != '':
            self.Pronunciation = pronunciation
        phone = note.get_NotePhoneInfo()
        if phone is not None:
            self.EditedPhones = OpenSvipPhones().decode(phone)
        vibrato = note.get_Vibrato()
        if vibrato is not None:
            self.Vibrato = OpenSvipVibrato().decode(note)
        return self

    def dedict(self, obj: dict):
        self.StartPos = obj['StartPos']
        self.Length = obj['Length']
        self.KeyNumber = obj['KeyNumber']
        self.HeadTag = obj['HeadTag']
        self.Lyric = obj['Lyric']
        self.Pronunciation = obj['Pronunciation']
        if obj['EditedPhones'] is not None:
            self.EditedPhones = OpenSvipPhones().dedict(obj['EditedPhones'])
        if obj['Vibrato'] is not None:
            self.Vibrato = OpenSvipVibrato().dedict(obj['Vibrato'])
        return self

    def encode(self) -> Note:
        note = Note()
        note.set_ActualStartPos(self.StartPos)
        note.set_WidthPos(self.Length)
        note.set_KeyIndex(self.KeyNumber + 12)
        note.set_HeadTag(OpenSvipNoteHeadTags.get_index(self.HeadTag))
        note.set_Lyric(self.Lyric)
        note.set_Pronouncing(self.Pronunciation)
        if self.EditedPhones is not None:
            note.set_NotePhoneInfo(self.EditedPhones.encode())
        if self.Vibrato is not None:
            percent, vibrato = self.Vibrato.encode()
            note.set_VibratoPercentInfo(percent)
            note.set_Vibrato(vibrato)
        return note


class OpenSvipPhones:
    def __init__(self):
        self.HeadLengthInSecs: float = -1.0
        self.MidRatioOverTail: float = -1.0

    def decode(self, phone: NotePhoneInfo):
        self.HeadLengthInSecs = phone.get_HeadPhoneTimeInSec()
        self.MidRatioOverTail = phone.get_MidPartOverTailPartRatio()
        return self

    def dedict(self, obj: dict):
        self.HeadLengthInSecs = obj['HeadLengthInSecs']
        self.MidRatioOverTail = obj['MidRatioOverTail']
        return self

    def encode(self) -> NotePhoneInfo:
        phone = NotePhoneInfo()
        phone.set_HeadPhoneTimeInSec(self.HeadLengthInSecs)
        phone.set_MidPartOverTailPartRatio(self.MidRatioOverTail)
        return phone


class OpenSvipVibrato:
    def __init__(self):
        self.StartPercent: float = 0.
        self.EndPercent: float = 0.
        self.IsAntiPhase: bool = False
        self.Amplitude: OpenSvipParamCurve = OpenSvipParamCurve()
        self.Frequency: OpenSvipParamCurve = OpenSvipParamCurve()

    def decode(self, note: Note):
        percent = note.get_VibratoPercentInfo()
        if percent is not None:
            self.StartPercent = percent.get_StartPercent()
            self.EndPercent = percent.get_EndPercent()
        elif note.get_VibratoPercent() > 0:
            self.StartPercent = 1. - note.get_VibratoPercent() / 100.
            self.EndPercent = 1.
        vibrato = note.get_Vibrato()
        self.IsAntiPhase = vibrato.get_IsAntiPhase()
        self.Amplitude.decode(vibrato.get_AmpLine())
        self.Frequency.decode(vibrato.get_FreqLine())
        return self

    def dedict(self, obj: dict):
        self.StartPercent = obj['StartPercent']
        self.EndPercent = obj['EndPercent']
        self.IsAntiPhase = obj['IsAntiPhase']
        self.Amplitude.dedict(obj['Amplitude'])
        self.Frequency.dedict(obj['Frequency'])
        return self

    def encode(self) -> Tuple[VibratoPercentInfo, VibratoStyle]:
        percent = VibratoPercentInfo()
        percent.set_StartPercent(self.StartPercent)
        percent.set_EndPercent(self.EndPercent)
        vibrato = VibratoStyle()
        vibrato.set_IsAntiPhase(self.IsAntiPhase)
        vibrato.AmpLine = self.Amplitude.encode(left=-1, right=100001)
        vibrato.FreqLine = self.Frequency.encode(left=-1, right=100001)
        return percent, vibrato


class OpenSvipParams:
    def __init__(self):
        self.Pitch: OpenSvipParamCurve = OpenSvipParamCurve()
        self.Volume: OpenSvipParamCurve = OpenSvipParamCurve()
        self.Breath: OpenSvipParamCurve = OpenSvipParamCurve()
        self.Gender: OpenSvipParamCurve = OpenSvipParamCurve()
        self.Strength: OpenSvipParamCurve = OpenSvipParamCurve()

    def decode(self, track: SingingTrack):
        if track.get_EditedPitchLine() is not None:
            self.Pitch.decode(track.get_EditedPitchLine(), op=lambda x: x - 1150 if x > 1050 else -100)
        if track.get_EditedVolumeLine() is not None:
            self.Volume.decode(track.get_EditedVolumeLine())
        if track.get_EditedBreathLine() is not None:
            self.Breath.decode(track.get_EditedBreathLine())
        if track.get_EditedGenderLine() is not None:
            self.Gender.decode(track.get_EditedGenderLine())
        if track.get_EditedPowerLine() is not None:
            self.Strength.decode(track.get_EditedPowerLine())
        return self

    def dedict(self, obj: dict):
        self.Pitch.dedict(obj['Pitch'])
        self.Volume.dedict(obj['Volume'])
        self.Breath.dedict(obj['Breath'])
        self.Gender.dedict(obj['Gender'])
        self.Strength.dedict(obj['Strength'])
        return self

    def encode(self) -> Dict[str, LineParam]:
        return {
            'Pitch': self.Pitch.encode(op=lambda x: x + 1150 if x > -100 else -100),
            'Volume': self.Volume.encode(),
            'Breath': self.Breath.encode(),
            'Gender': self.Gender.encode(),
            'Strength': self.Strength.encode()
        }


class OpenSvipParamCurve:
    def __init__(self):
        self.TotalPointsCount: int = 0
        self.PointList: List[Tuple[int]] = []

    def decode(self, line: LineParam, op=lambda x: x):
        self.TotalPointsCount = line.Length()
        point = line.get_Begin()
        for _ in range(self.TotalPointsCount):
            value = point.get_Value()
            self.PointList.append((value.get_Pos(), op(value.get_Value())))
            point = point.get_Next()
        return self

    def dedict(self, obj: dict):
        self.TotalPointsCount = obj['TotalPointsCount']
        for ele in obj['PointList']:
            self.PointList.append((ele[0], ele[1]))
        return self

    def encode(self, op=lambda x: x, left=-192000, right=1073741823, default=0) -> LineParam:
        line = LineParam()
        length = len(self.PointList)
        self.PointList = sorted(self.PointList, key=lambda x: x[0])
        for p in self.PointList:
            if left <= p[0] <= right:
                node = LineParamNode()
                node.set_Pos(p[0])
                node.set_Value(op(p[1]))
                line.PushBack(node)
        if length == 0 or line.get_Begin().get_Value().get_Pos() > left:
            bound = LineParamNode()
            bound.set_Pos(left)
            bound.set_Value(default)
            line.PushFront(bound)
        if length == 0 or line.get_Back().get_Pos() < right:
            bound = LineParamNode()
            bound.set_Pos(right)
            bound.set_Value(default)
            line.PushBack(bound)
        self.TotalPointsCount = line.Length()
        return line
