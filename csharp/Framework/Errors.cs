using System.ComponentModel;

namespace OpenSvip.Framework
{
    public enum ErrorTypes
    {
        [Description("准备转换时")] Prepare,
        [Description("导入工程时")] Import,
        [Description("导出工程时")] Export
    }
}
