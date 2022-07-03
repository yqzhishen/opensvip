namespace FlutyDeer.LyricsPlugin
{
    public class MetaInfoLine
    {
        public MetaInfoType Type { get; set; }
        public string Value { get; set; }

        public MetaInfoLine()
        {
            
        }

        public MetaInfoLine(MetaInfoType type, string value)
        {
            Type = type;
            Value = value;
        }

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
                case MetaInfoType.By:
                    return "by";
                case MetaInfoType.Offset:
                    return "offset";
                default:
                    return "";
            }
        }
    }
}
