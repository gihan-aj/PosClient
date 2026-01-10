using System.Text.Json.Serialization;

namespace PosClient.Desktop.Shared
{
    public record CreateResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; init; }
    }
}
