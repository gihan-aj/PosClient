using System.Text.Json.Serialization;

namespace PosClient.Desktop.Infrastructure.Network
{
    public class ProblemDetails
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; } = string.Empty;

        [JsonPropertyName("errors")]
        public Dictionary<string, string[]?> Errors { get; set; } = new();

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;
    }
}
