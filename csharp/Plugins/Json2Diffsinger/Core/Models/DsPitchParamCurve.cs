using System.Collections.Generic;

namespace Json2DiffSinger.Core.Models
{
    /// <summary>
    /// ds 音高参数曲线
    /// </summary>
    public class DsPitchParamCurve
    {
        /// <summary>
        /// F0 步长
        /// </summary>
        public float F0TimeStepSize { get; set; } = 0.005f;

        /// <summary>
        /// 参数点列表
        /// </summary>
        public List<DsParamNode> PointList { get; set; } = new List<DsParamNode>();
    }
}
