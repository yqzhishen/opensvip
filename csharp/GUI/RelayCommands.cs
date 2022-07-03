using System.Diagnostics;

namespace OpenSvip.GUI
{
    public static class RelayCommands
    {

        public static readonly RelayCommand<string> StartProcessCommand = new RelayCommand<string>(
            p => true,
            p => Process.Start(p));

    }
}
