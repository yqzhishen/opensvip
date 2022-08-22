using Json2DiffSinger.Core.Converters;
using Json2DiffSinger.Core.Models;
using Json2DiffSinger.Options;
using OpenSvip.Model;

namespace Json2DiffSinger
{
    public class DiffSingerEncoder
    {
        /// <summary>
        /// 参数模式选项
        /// </summary>
        public ModeOption InputModeOption {get;set;}

        /// <summary>
        /// 格式化 JSON 代码选项
        /// </summary>
        public bool IsFormatted {get;set;}

        /// <summary>
        /// 转为 ds 参数
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public AbstractParamsModel EncodeParams(Project project)
        {
            AbstractParamsModel result = null;
            switch (InputModeOption)
            {
                case ModeOption.Note://这个选项已经在 Properties.xml 注释掉了
                    result = ChineseCharactersParamsEncoder.Encode(project, IsFormatted);
                    break;
                case ModeOption.ManualPhoneme://音素（有参）
                case ModeOption.AutoPhoneme://音素（无参）
                    result = PhonemeParamsEncoder.Encode(project, IsFormatted, InputModeOption);
                    break;
            }
            return result;
        }
    }
}
