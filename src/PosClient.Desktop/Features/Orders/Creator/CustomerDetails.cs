namespace PosClient.Desktop.Features.Orders.Creator
{
    public class CustomerDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? Region { get; set; }
        public string? PostalCode { get; set; }
    }
}
