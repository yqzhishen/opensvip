using System.Runtime.InteropServices;

namespace BinSvip.Standalone.Nrbf
{
    internal static unsafe class NrbfLibrary
    {
        public enum xs_note_head_tag
        {
            NO_TAG,
            SPL_TAG,
            SP_TAG,
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_string
        {
            public byte* str;
            public int size;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_node
        {
            public void* data;
            public xs_node* next;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_overlappable
        {
            public bool Overlapped;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_beat_size
        {
            public int x;
            public int y;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_song_tempo
        {
            public xs_overlappable @base;

            public int pos;
            public int tempo;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_song_beat
        {
            public xs_overlappable @base;

            public int barIndex;
            public xs_beat_size beatSize;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_line_param
        {
            public int Pos;
            public int Value;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_vibrato_style
        {
            public bool IsAntiPhase;

            // Linked list
            public xs_node* ampLine;
            public xs_node* freqLine;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_vibrato_percent_info
        {
            public float startPercent;
            public float endPercent;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_note_phone_info
        {
            public float HeadPhoneTimeInSec;
            public float MidPartOverTailPartRatio;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_note
        {
            public xs_overlappable @base;

            public xs_note_phone_info* NotePhoneInfo;
            public int VibratoPercent;
            public xs_vibrato_style* Vibrato;
            public xs_vibrato_percent_info* VibratoPercentInfo;

            public int startPos;
            public int widthPos;
            public int keyIndex;

            public xs_string lyric;
            public xs_string pronouncing;
            public xs_note_head_tag headTag;
        };

        public enum xs_track_type
        {
            SINGING,
            INSTRUMENT,
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_track
        {
            public double volume;
            public double pan;
            public xs_string name;
            public bool mute;
            public bool solo;
            public xs_track_type track_type;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_singing_track
        {
            public xs_track @base;
            public xs_string AISingerId;

            // Linked list
            public xs_node* noteList;

            public bool needRefreshBaseMetadataFlag;

            // Linked list
            public xs_node* editedPitchLine;
            public xs_node* editedVolumeLine;
            public xs_node* editedBreathLine;
            public xs_node* editedGenderLine;
            public xs_node* editedPowerLine;
            public int reverbPreset;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_instrument_track
        {
            public xs_track @base;

            public double SampleRate;
            public int SampleCount;
            public int ChannelCount;
            public int OffsetInPos;

            public xs_string InstrumentFilePath;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct xs_app_model
        {
            public xs_string ProjectFilePath;

            // Linked list
            public xs_node* tempoList;
            public xs_node* beatList;
            public xs_node* trackList;

            public int quantize;
            public bool isTriplet;
            public bool isNumericalKeyName;
            public int firstNumericalKeyNameAtIndex;
        };

        public enum qnrbf_stream_status
        {
            OK,
            READ_PAST_END,
            READ_CORRUPT_DATA,
            WRITE_FAILED,
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct qnrbf_xstudio_context
        {
            public qnrbf_stream_status status;
            public xs_string error;
            public xs_string buf;
            public xs_app_model* model;
        };

        private static int _referenceCount = 0;

        /* Load or free libraries at correct time */
        public static void Load()
        {
            if (_referenceCount == 0)
            {
                NrbfLibraryImpl.Init();
                
                // Init dll
                qnrbf_dll_init();
            }

            // Console.WriteLine($"Library ref cnt: {_referenceCount}");

            _referenceCount++;
        }

        public static void Unload()
        {
            // _referenceCount--;
            // if (_referenceCount == 0)
            // {
            //     // Exit dll
            //     qnrbf_dll_exit();
            //     
            //     NrbfLibraryImpl.Deinit();
            // }
        }


        /* You must call the following functions when loading or freeing this library,
        * to make it thread-safe for your application.
        */

        public static void qnrbf_dll_init() => NrbfLibraryImpl.qnrbf_dll_init_ptr();

        public static void qnrbf_dll_exit() => NrbfLibraryImpl.qnrbf_dll_exit_ptr();


        /* Allocator */
        public static void* qnrbf_malloc(int size) => NrbfLibraryImpl.qnrbf_malloc_ptr(size);

        public static void qnrbf_free(void* data) => NrbfLibraryImpl.qnrbf_free_ptr(data);

        public static void qnrbf_memcpy(void* dst, void* src, int count) =>
            NrbfLibraryImpl.qnrbf_memcpy_ptr(dst, src, count);

        public static void qnrbf_memset(void* dst, int value, int count) =>
            NrbfLibraryImpl.qnrbf_memset_ptr(dst, value, count);

        /* Context */
        public static qnrbf_xstudio_context* qnrbf_xstudio_alloc_context() =>
            NrbfLibraryImpl.qnrbf_xstudio_alloc_context_ptr();

        public static void qnrbf_xstudio_free_context(qnrbf_xstudio_context* ctx) =>
            NrbfLibraryImpl.qnrbf_xstudio_free_context_ptr(ctx);

        public static void qnrbf_xstudio_read(qnrbf_xstudio_context* ctx) =>
            NrbfLibraryImpl.qnrbf_xstudio_read_ptr(ctx);

        public static void qnrbf_xstudio_write(qnrbf_xstudio_context* ctx) =>
            NrbfLibraryImpl.qnrbf_xstudio_write_ptr(ctx);
    }
}
