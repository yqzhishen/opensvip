using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using OpenSvip.GUI.Config;
using Tomlet;

namespace OpenSvip.GUI
{
    public class UpdateChecker
    {
        private readonly string _updateUri;
        
        public UpdateChecker(string updateUri = Information.UpdateLogUrl)
        {
            _updateUri = updateUri;
        }
        
        public bool CheckForUpdate(out UpdateLog updateLog, string currentVersion = Information.ApplicationVersion)
        {
            if (_updateUri == null)
            {
                updateLog = null;
                return false;
            }
            var timer = new Thread(() =>
            {
                Thread.Sleep(1000);
            });
            timer.Start();
            var request = (HttpWebRequest)WebRequest.Create(_updateUri);
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
            var curVersion = new Version(currentVersion.Split(' ', '-')[0]);
            var newVersion = new Version(updateLog.Version.Split(' ', '-')[0]);
            timer.Join();
            return newVersion > curVersion;
        }
    }
}
