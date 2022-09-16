using System;

namespace FlutyDeer.Svip3Plugin
{
    public static class Constants
    {
        public const string TypeUrlBase = "type.googleapis.com/";

        public static class TypeNames
        {
            public const string SingingTrack = "xstudio.proto.SingingTrack";

            public const string AudioTrack = "xstudio.proto.AudioTrack";
        }

        public static class Colors
        {
            private static readonly string[] colors =
            {
                "#24936E",
                "#B54434",
                "#B5495B",
                "#A8497A",
                "#4F726C",
                "#939650",
                "#CA7A2C",
                "#DB4D6D",
                "#77428D",
                "#005CAF",
            };

            public static int Count
            {
                get => colors.Length;
            }

            public static string GetColor()
            {
                int randIndex = new Random().Next(colors.Length);
                return colors[randIndex];
            }

            public static string GetColor(int index)
            {
                return colors[index % colors.Length];
            }
        }
    }
}
