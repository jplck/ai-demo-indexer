using Newtonsoft.Json;

namespace Company.Function
{
    public class DocumentContentResult
    {
        [JsonProperty("documentUri")]
        public required string DocumentUri { get; set; }
        [JsonProperty("content")]
        public required string Content { get; set; }
    }
}