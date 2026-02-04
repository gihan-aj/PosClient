namespace PosClient.Desktop.Features.Orders
{
    public enum OrderStatus
    {
        Pending = 1,           // Just created, not confirmed
        Confirmed = 2,         // Confirmed, items reserved/in production
        ReadyToPack = 3,       // All items ready, can pack now
        Packed = 4,            // Packed, ready for courier
        Shipped = 5,           // With courier
        Delivered = 6,         // Customer received
        Cancelled = 0          // Order cancelled
    }
}
