namespace FlutyDeer.LyricsPlugin
{
    public class MetaInfoLine
    {
        public MetaInfoType Type { get; set; }
        public string Value { get; set; } = "";

        public override string ToString()
        {
            return $"[{TypeToSting(Type)}:{Value}]";
        }
        private string TypeToSting(MetaInfoType type)
        {
            switch (type)
            {
                case MetaInfoType.Title:
                    return "ti";
                case MetaInfoType.Artist:
                    return "ar";
                case MetaInfoType.Album:
                    return "al";
                case MetaInfoType.Author:
                    return "by";
                case MetaInfoType.Offset:
                    return "offset";
                default:
                    return "";
            }
        }
    }
}
