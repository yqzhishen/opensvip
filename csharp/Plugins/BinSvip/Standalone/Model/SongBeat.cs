namespace BinSvip.Standalone.Model
{
    public class BeatSize
    {
        public BeatSize() : this(0, 0)
        {
        }

        public BeatSize(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /* Members */
        public int x;
        public int y;
    }

    public class SongBeat : IOverlappable
    {
        public SongBeat()
        {
            barIndex = 0;
            beatSize = new BeatSize();
        }

        public SongBeat(int barIndex, int x, int y)
        {
            this.barIndex = barIndex;
            beatSize = new BeatSize(x, y);
        }

        /* Members */
        public int barIndex;
        public BeatSize beatSize;
    }
}