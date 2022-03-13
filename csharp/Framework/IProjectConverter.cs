using OpenSvip.Model;

namespace OpenSvip.Framework
{
    /// <summary>
    /// OpenSVIP Framework 核心接口。所有工程格式的转换器实现本接口并接入框架后，可与 OpenSVIP Model 互相转换。
    /// </summary>
    public interface IProjectConverter
    {
        /// <summary>
        /// 读取目标工程文件并转换为 OpenSVIP Model 对象。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>目标格式对象</returns>
        Project Load(string path);

        /// <summary>
        /// 将 OpenSVIP Model 对象转换并保存为目标格式工程文件。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="project">OpenSVIP Model 对象</param>
        void Save(string path, Project project);
    }
}
