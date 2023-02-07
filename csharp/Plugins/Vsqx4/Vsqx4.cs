using System.Xml.Schema;
using System.Xml.Serialization;

namespace Plugin.VSQX
{
    [XmlRoot(Namespace = @"http://www.yamaha.co.jp/vocaloid/schema/vsq4/")]
    public class Vsqx4
    {
        [XmlElement(Form = XmlSchemaForm.Qualified)]
        public string schemaLocation = @"http://www.yamaha.co.jp/vocaloid/schema/vsq4/ vsq4.xsd";

        [XmlElement]
        public string vender = @"<![CDATA[Yamaha corporation]]>";

        [XmlElement]
        public string version = @"<![CDATA[4.0.0.3]]>";

        public TvVoiceTable vVoiceTable;
        public Tmixer mixer;
    }

    public class TvVoiceTable
    {
        [XmlArray]
        public TvVoice[] vVoice;

        public class TvVoice
        {
            public byte bs = 4;
            public byte pc;
            public string id = @"<![CDATA[BETDB8W6KWZPYEB9]]>";
            public string name = @"<![CDATA[Tianyi_CHN]]>";
            public TvPrm tvPrm;

            public class TvPrm
            {
                /// 呼吸度
                public byte bre;

                /// 明亮度
                public byte bri;

                /// 清晰度
                public byte cle;

                /// 性别值
                public byte gen;

                /// 开口度
                public byte ope;
            }
        }
    }

    public class Tmixer
    {
        public TmasterUnit masterUnit;

        [XmlArray]
        public TvsUnit[] vsUnit;

        public TmonoUnit monoUnit;
        public TstUnit stUnit;

        public class TmasterUnit
        {
            public short oDev;
            public short rLvl;
            public short vol;
        }

        public class TvsUnit
        {
            [XmlIgnore]
            private static byte Index;

            /// 轨道序号
            public short tNo;

            public short iGin;
            public short sLvl = -898;
            public short sEnable;
            public short m;
            public short s;
            public short pan = 64;
            public short vol;

            public TvsUnit()
            {
                tNo = Index++;
            }
        }

        public class TmonoUnit
        {
            public short iGin;
            public short sLv = -898;
            public short sEnable;
            public short m;
            public short s;
            public short pan;
            public short vol;
        }

        public class TstUnit
        {
            public short iGin;
            public short m;
            public short s;
            public short vol = -129;
        }
    }

    public class TmasterTrack
    {
    }
}