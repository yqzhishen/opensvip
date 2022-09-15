using Newtonsoft.Json;

namespace Json2DiffSinger.Core.Models
{
    public abstract class AbstractParamsModel
    {
        [JsonProperty("offset")]
        public double Offset { get; set; } = 0;

        [JsonProperty(PropertyName = "seed", NullValueHandling = NullValueHandling.Ignore)]
        public int? Seed { get; set; } = null;
    }
}