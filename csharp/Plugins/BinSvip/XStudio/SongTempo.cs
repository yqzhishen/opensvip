namespace XSAppModel.XStudio
{

    public class SongTempo : IOverlappable
    {
        public SongTempo() : this(0, 120)
        {
        }

        public SongTempo(int pos, int tempo)
        {
            this.pos = pos;
            this.tempo = tempo;
        }

        /* Members */
        public int pos;
        public int tempo;
    }
}