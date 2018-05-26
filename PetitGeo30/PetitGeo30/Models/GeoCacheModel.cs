using Newtonsoft.Json;

namespace PetitGeo30.Models
{
    public class GeoCacheModel
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_rev")]
        public string Rev { get; set; }

        [JsonProperty("geoCacheId")]
        public int GeoCacheId { get; set; }

        [JsonProperty("geoCacheHiddenTimestamp")]
        public long GeoCacheHiddenTimestamp { get; set; }

        [JsonProperty("geoCacheLatitude")]
        public double GeoCacheLatitude { get; set; }

        [JsonProperty("geoCacheLongitude")]
        public double GeoCacheLongitude { get; set; }
    }
}
