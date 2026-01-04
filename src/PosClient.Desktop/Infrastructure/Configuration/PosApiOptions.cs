namespace PosClient.Desktop.Infrastructure.Configuration
{
    public class PosApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
