using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    [ProtoContract]
    public class Xs3Tempo
    {
        #region Properties & Fields

        [ProtoMember(2)]
        private int _bpm;

        public float Tempo
        {
            get => _bpm / 100.0f;
            set => _bpm = (int)(value * 100);
        }

        #endregion
    }
}
