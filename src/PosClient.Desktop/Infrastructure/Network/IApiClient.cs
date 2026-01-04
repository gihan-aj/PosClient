using PosClient.Desktop.Shared;

namespace PosClient.Desktop.Infrastructure.Network
{
    public interface IApiClient
    {
        Task<Result<TResponse>> GetAsync<TResponse>(string endpoint);

        Task<Result<TResponse>> PostAsync<TResponse>(string endpoint, object data);

        Task<Result> PostAsync(string endpoint, object data);

        Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, object data);

        Task<Result> PutAsync(string endpoint, object data);

        Task<Result<TResponse>> DeleteAsync<TResponse>(string endpoint);

        Task<Result> DeleteAsync(string endpoint);
    }
}
