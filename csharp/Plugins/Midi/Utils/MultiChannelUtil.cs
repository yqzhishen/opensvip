using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlutyDeer.MidiPlugin.Utils
{
    public static class MultiChannelUtil
    {
        public static List<SevenBitNumber> GetChannels(string optionString)
        {
            var channels = new List<SevenBitNumber>();
            var channelStrings = optionString.Split(',');
            foreach(var item in channelStrings)
            {
                try
                {
                    if (item.Contains("-"))//范围
                    {
                        var range = item.Split('-');
                        var start = int.Parse(range[0]);
                        var end = int.Parse(range[1]);
                        for (int i = start; i <= end; i++)
                        {
                            channels.Add((SevenBitNumber)(i - 1));
                        }
                    }
                    else
                    {
                        channels.Add((SevenBitNumber)(int.Parse(item) - 1));
                    }
                }
                catch
                {
                    
                }
            }
            return channels;
        }
    }
}
