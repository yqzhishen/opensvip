using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Plugin.Dspx.Model;

namespace Plugin.Dspx
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dspx = DspxModel.Read(args[0]);
            Console.WriteLine(dspx);
        }
    }
}
