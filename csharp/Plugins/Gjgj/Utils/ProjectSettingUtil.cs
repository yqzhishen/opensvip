using FlutyDeer.GjgjPlugin.Model;

namespace FlutyDeer.GjgjPlugin.Utils
{
    public static class ProjectSettingUtil
    {
        /// <summary>
        /// 设置工程的基本属性。
        /// </summary>
        /// <returns>工程的基本属性。</returns>
        public static GjProjectSetting EncodeProjectSetting()
        {
            GjProjectSetting gjProjectSetting = new GjProjectSetting
            {
                No1KeyName = "C",
                EQAfterMix = "",
                ProjectType = 0,
                Denominator = 4,
                SynMode = 0
            };
            return gjProjectSetting;
        }
    }
}
