using OpenSvip.Stream;

namespace OpenSvip.Console;

internal static class ConsoleApp
{
    public static void Main(string[] args)
    {
        var project = Binary.Read(@"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\Dev Projects\opensvip\test\黏黏黏黏.svip");
        Json.Dump(@"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\Dev Projects\opensvip\test\csharp.json", project, indented: true);
        project = Json.Load(@"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\Dev Projects\opensvip\test\csharp.json");
        Binary.Write(@"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\Dev Projects\opensvip\test\repack.svip", project);
        Json.Dump(@"E:\YQ数据空间\YQ实验室\实验室：XStudioSinger\Dev Projects\opensvip\test\repack.json", project, indented: true);
    }
}
