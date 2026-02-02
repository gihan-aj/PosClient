namespace PosClient.Desktop.Shared
{
    public enum PaymentStatus
    {
        Pending = 1,    // Online payment initiated, waiting for webhook
        Completed = 2,  // Money successfully captured (Cash or Online success)
        Failed = 3,     // Card declined
        Voided = 4,     // Cashier mistake, transaction ignored
        Refunded = 5    // Specific status for refund records
    }
}
