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
            var dspx = DspxModel.Read(@"D:\Users\fluty\Documents\GitHub\qsynthesis-docs\999. 临时文档\dspx-example.json");
            dspx.Write(@"D:\Users\fluty\Documents\GitHub\qsynthesis-docs\999. 临时文档\dspx-example-out.json");
        }
    }
}
