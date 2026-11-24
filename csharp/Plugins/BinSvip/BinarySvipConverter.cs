using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using BinSvip.Standalone;
using Microsoft.Win32;
using OpenSvip.Framework;
using OpenSvip.Model;

namespace BinSvip.Stream
{
    public class BinarySvipConverter : IProjectConverter
    {

        public Project Load(string path, ConverterOptions options)
        {
            return new StandaloneSvipConverter().Load(path, options);
        }

        public void Save(string path, Project project, ConverterOptions options)
        {
            new StandaloneSvipConverter().Save(path, project, options);
            return;    
        }
    }
}