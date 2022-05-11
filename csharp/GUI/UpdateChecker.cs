using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using OpenSvip.GUI.Config;
using Tomlet;

namespace OpenSvip.GUI
{
    public class UpdateChecker
    {
        public bool CheckForUpdate(out UpdateLog updateLog)
        {
            var timer = new Thread(() =>
            {
                Thread.Sleep(1000);
            });
            timer.Start();
            var request = (HttpWebRequest)WebRequest.Create(Information.UpdateLogUrl);
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpException($"Unexpected response code: {response.StatusCode}");
            }
            var stream = response.GetResponseStream();
            if (stream == null)
            {
                throw new HttpException("Response is null");
            }
            using (var reader = new StreamReader(stream))
            {
                var responseBody = reader.ReadToEnd();
                updateLog = TomletMain.To<UpdateLog>(responseBody);
            }
            var currentVersion = new Version(Information.ApplicationVersion.Split(' ')[0]);
            var newVersion = new Version(updateLog.Version.Split(' ')[0]);
            timer.Join();
            return newVersion > currentVersion;
        }
    }
}
