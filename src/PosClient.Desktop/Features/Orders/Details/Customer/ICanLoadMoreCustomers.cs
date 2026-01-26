using CommunityToolkit.Mvvm.Input;

namespace PosClient.Desktop.Features.Orders.Details.Customer
{
    public interface ICanLoadMoreCustomers
    {
        IAsyncRelayCommand LoadNextCustomerListCommand { get; }
    }
}
