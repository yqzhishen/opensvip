using OpenSvip.Model;

namespace OpenSvip.Adapter
{
    /// <summary>
    /// OpenSVIP Framework 核心接口。所有工程格式的转换器实现本接口以接入框架后，可与 OpenSVIP Model 互相转换。
    /// </summary>
    /// <typeparam name="T">目标工程格式的对象模型</typeparam>
    public interface IProjectConverter<T>
    {
        /// <summary>
        /// 读取目标工程文件并生成目标格式对象。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>目标格式对象</returns>
        T Load(string path);

        /// <summary>
        /// 将目标格式对象保存到目标工程文件。
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="model">目标格式对象</param>
        void Save(string path, T model);
        
        /// <summary>
        /// 将目标格式对象解析为 OpenSVIP Model 对象。
        /// </summary>
        /// <param name="model">目标格式对象</param>
        /// <returns>OpenSVIP Model 对象</returns>
        Project Parse(T model);

        /// <summary>
        /// 从 OpenSVIP Model 对象构建目标格式对象。
        /// </summary>
        /// <param name="project">OpenSVIP Model 对象</param>
        /// <returns>目标格式对象。</returns>
        T Build(Project project);
    }
}
