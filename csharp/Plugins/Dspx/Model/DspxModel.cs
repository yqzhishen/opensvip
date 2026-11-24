using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace Plugin.Dspx.Model
{
    /// <summary>
    /// DSPX 格式根区域
    /// </summary>
    public class DspxModel
    {
        #region Properties

        [JsonProperty("metadata")]
        public DsMetadata Metadata { get; set; }

        [JsonProperty("content")]
        public DsContent Content { get; set; }

        [JsonProperty("workspace")]
        public DsWorkspace Workspace { get; set; }

        #endregion Properties

        #region Methods

        public static DspxModel Read(string path)
        {
            var model = new DspxModel();
            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
                model = JsonConvert.DeserializeObject<DspxModel>(reader.ReadToEnd());
            return model;
        }

        public void Write(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        #endregion Methods
    }
}