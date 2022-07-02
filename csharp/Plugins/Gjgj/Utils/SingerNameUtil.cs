using FlutyDeer.GjgjPlugin.Model;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class SingerNameUtil
    {
        /// <summary>
        /// 根据歌手名称返回歌手代号。
        /// </summary>
        public static string ToSingerCode(string singerName)
        {
            switch (singerName)
            {
                case "扇宝":
                    return "513singer";
                case "SING-林嘉慧":
                    return "514singer";
                case "Rocky":
                    return "881singer";
                case "超越AI":
                    return "ycysinger";
                default:
                    return "513singer";
            }
        }

        /// <summary>
        /// 根据歌手代号返回歌手名称。
        /// </summary>
        /// <param name="index">演唱轨索引。</param>
        /// <returns></returns>
        public static string GetSingerName(GjSingingTrack gjSingingTrack)
        {
            switch (gjSingingTrack.Name)
            {
                case "513singer":
                    return "扇宝";
                case "514singer":
                    return "SING-林嘉慧";
                case "881singer":
                    return "Rocky";
                case "ycysinger":
                    return "超越AI";
                default:
                    return GetUserMadeSingerName(gjSingingTrack);
            }
        }

        /// <summary>
        /// 返回用户自制歌手名称。
        /// </summary>
        /// <param name="index">演唱轨索引。</param>
        /// <returns></returns>
        public static string GetUserMadeSingerName(GjSingingTrack gjSingingTrack)
        {
            string singerName = "演唱轨";
            if (gjSingingTrack.SingerInfo != null)
            {
                singerName = gjSingingTrack.SingerInfo.SingerName;
            }
            return singerName;
        }
    }
}