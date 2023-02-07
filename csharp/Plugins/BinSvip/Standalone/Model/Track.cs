using System.Collections.Generic;

namespace BinSvip.Standalone.Model
{

    public class ITrack
    {
        public ITrack() : this(1)
        {
        }

        public ITrack(double volume)
        {
            this.volume = volume;
            pan = 0;
            mute = false;
            solo = false;

            name = "";
        }

        /* Members */
        public double volume;
        public double pan;
        public string name;
        public bool mute;
        public bool solo;
    }

    public class SingingTrack : ITrack
    {
        public SingingTrack() : base(0.7)
        {
            needRefreshBaseMetadataFlag = false;
            reverbPreset = ReverbPreset.NONE;

            AISingerId = "";

            noteList = new List<Note>();

            editedPitchLine = null;
            editedVolumeLine = null;
            editedBreathLine = null;
            editedGenderLine = null;
            editedPowerLine = null;
        }

        /* Properties */
        public string AISingerId;

        /* Members */
        public List<Note> noteList;

        public bool needRefreshBaseMetadataFlag;

        public LineParam editedPitchLine;
        public LineParam editedVolumeLine;
        public LineParam editedBreathLine;
        public LineParam editedGenderLine;
        public LineParam editedPowerLine;

        public ReverbPreset reverbPreset;
    };

    public class InstrumentTrack : ITrack
    {
        public InstrumentTrack()
            : base(0.3)
        {
            SampleRate = 0;
            SampleCount = 0;
            ChannelCount = 0;
            OffsetInPos = 0;

            InstrumentFilePath = "";
        }

        /* Properties */
        public double SampleRate;
        public int SampleCount;
        public int ChannelCount;
        public int OffsetInPos;
        public string InstrumentFilePath;
    };
}