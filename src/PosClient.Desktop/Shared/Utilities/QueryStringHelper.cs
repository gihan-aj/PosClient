namespace PosClient.Desktop.Shared.Utilities
{
    public static class QueryStringHelper
    {
        public static string ToQueryString(object request)
        {
            if (request == null) return string.Empty;

            var properties = request.GetType().GetProperties()
                .Where(p => p.GetValue(request, null) != null)
                .Select(p =>
                {
                    var value = p.GetValue(request, null);
                    var name = char.ToLowerInvariant(p.Name[0]) + p.Name.Substring(1); // camelCase

                    // Handle generic lists if necessary, but for simple types:
                    return $"{name}={Uri.EscapeDataString(value!.ToString()!)}";
                });

            return "?" + string.Join("&", properties);
        }
    }
}
