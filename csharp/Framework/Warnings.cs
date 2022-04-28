using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace OpenSvip.Framework
{
    public enum WarningTypes
    {
        /// <summary>
        /// 代表警告信息由音符和谱面产生，例如非法的音高或音符重叠
        /// </summary>
        [Description("音符")] Notes,
        
        /// <summary>
        /// 代表警告信息由歌词产生，例如非法的字符或发音
        /// </summary>
        [Description("歌词")] Lyrics,
        
        /// <summary>
        /// 代表警告信息由参数产生，例如非法的参数曲线和参数点
        /// </summary>
        [Description("参数")] Params,
        
        /// <summary>
        /// 代表警告信息由除其他几种情况以外的其他因素产生
        /// </summary>
        [Description("未知")] Others
    }
    
    [Serializable]
    public class Warning
    {
        public WarningTypes Type;
        public string Location;
        public string Message;

        public override string ToString()
        {
            var type = typeof(WarningTypes);
            var ty = type.GetField(type.GetEnumName(Type)).GetCustomAttribute<DescriptionAttribute>().Description;
            var loc = string.IsNullOrWhiteSpace(Location) ? "" : Location + ": ";
            return $"[{ty}] {loc}{Message}";
        }
    }

    public static class Warnings
    {
        private static readonly List<Warning> WarningList = new List<Warning>();

        /// <summary>
        /// 添加一条警告信息，适用于遇到了不合法的工程格式或数据，但程序尚能纠正处理的情况。
        /// </summary>
        /// <param name="message">警告信息正文内容</param>
        /// <param name="location">警告信息产生的位置</param>
        /// <param name="type">警告信息的类别</param>
        /// <example><code>Warnings.AddWarning("非法的拼音音节", "第 1 轨道第 14 小节", "WarningTypes.Lyrics")</code></example>
        public static void AddWarning(
            string message,
            string location = null,
            WarningTypes type = WarningTypes.Others)
        {
            WarningList.Add(new Warning
            {
                Type = type,
                Location = location,
                Message = message
            });
        }
        
        /// <summary>
        /// 获取已添加的所有警告。
        /// </summary>
        public static Warning[] GetWarnings()
        {
            return WarningList.ToArray();
        }

        /// <summary>
        /// 清除已添加的所有警告。
        /// </summary>
        public static void ClearWarnings()
        {
            WarningList.Clear();
        }
    }
}
