using ProtoBuf;

namespace FlutyDeer.Svip3Plugin.Model
{
    /// <summary>
    /// XS 3 曲速
    /// </summary>
    [ProtoContract]
    public class Xs3Tempo
    {
        #region Properties & Fields

        /// <summary>
        /// XS 3 内的曲速（100倍于实际值）
        /// </summary>
        [ProtoMember(2)]
        private int _tempo;

        /// <summary>
        /// 曲速
        /// </summary>
        public float Tempo
        {
            get => _tempo / 100.0f;
            set => _tempo = (int)(value * 100);
        }

        #endregion
    }
}
