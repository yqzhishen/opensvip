using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSvip.GUI
{
    public static class RelayCommands
    {

        public static readonly RelayCommand<string> StartProcessCommand = new RelayCommand<string>(
            p => true,
            p => Process.Start(p));

    }
}
