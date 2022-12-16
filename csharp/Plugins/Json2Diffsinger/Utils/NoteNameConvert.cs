namespace Json2DiffSinger.Utils
{
    /// <summary>
    /// 音名转换工具。
    /// </summary>
    public static class NoteNameConvert
    {
        static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        /// <summary>
        /// 将 MIDI Key 转换为音名+八度。
        /// </summary>
        /// <param name="midiKeyNumber">MIDI Key</param>
        /// <returns></returns>
        public static string ToNoteName(int midiKeyNumber)
        {
            int octave = midiKeyNumber / 12 - 2 + 1;
            string name = NoteNames[midiKeyNumber % 12];
            return name + octave;
        }
    }
}
