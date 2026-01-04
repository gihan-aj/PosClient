using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using PosClient.Desktop.Shared;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace PosClient.Desktop.Infrastructure.Network
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;

        public ApiClient(
            HttpClient httpClient,
            ISnackbarService snackbarService, 
            IContentDialogService contentDialogService)
        {
            _httpClient = httpClient;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;
        }

        public async Task<Result<TResponse>> GetAsync<TResponse>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return HandleException<TResponse>(ex);
            }
        }

        public async Task<Result<TResponse>> PostAsync<TResponse>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                // the body is ignored for this generic method
                return await HandleResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return HandleException<TResponse>(ex);
            }
        }
        
        public async Task<Result> PostAsync(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                // the body is ignored for this generic method
                return await HandleVoidResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex).ToResult();
            }
        }

        public async Task<Result<TResponse>> PutAsync<TResponse>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                // the body is ignored for this generic method
                return await HandleResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return HandleException<TResponse>(ex);
            }
        }
        
        public async Task<Result> PutAsync(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                // the body is ignored for this generic method
                return await HandleVoidResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex).ToResult();
            }
        }

        public async Task<Result<TResponse>> DeleteAsync<TResponse>(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse<TResponse>(response);
            }
            catch (Exception ex)
            {
                return HandleException<TResponse>(ex);
            }
        }
        
        public async Task<Result> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleVoidResponse(response);
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex).ToResult();
            }
        }


        private async Task<Result<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return Result<T>.Success(default!);

                try
                {
                    var data = await response.Content.ReadFromJsonAsync<T>();
                    return Result<T>.Success(data!);
                }
                catch
                {
                    return Result<T>.Success(default!);
                }
            }

            await ShowErrorUi(response);
            return Result<T>.Failure(response.ReasonPhrase ?? "Unknown Error");
        }

        private async Task<Result> HandleVoidResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return Result.Success();

            await ShowErrorUi(response);
            return Result.Failure(response.ReasonPhrase ?? "Unknown Error");
        }

        private Result<T> HandleException<T>(Exception ex)
        {
            // Network errors, DNS errors, etc.
            _snackbarService.Show(
                "Connection Error",
                "Could not contact the server. Please check your connection.",
                ControlAppearance.Danger,
                new SymbolIcon(SymbolRegular.ErrorCircle24),
                TimeSpan.FromSeconds(5));

            return Result<T>.Failure(ex.Message);
        }

        private async Task ShowErrorUi(HttpResponseMessage response)
        {
            try
            {
                var error = await response.Content.ReadFromJsonAsync<ProblemDetails>();

                // CASE 1: Validation Errors (400 with errors dictionary)
                if (error?.Errors != null && error.Errors.Any())
                {
                    var sb = new StringBuilder();
                    foreach (var field in error.Errors)
                    {
                        foreach (var msg in field.Value!)
                        {
                            sb.AppendLine($"• {msg}");
                        }
                    }

                    // Show Modal Dialog for Validation
                    _ = _contentDialogService.ShowAlertAsync(
                        error.Title ?? "Validation Failed",
                        sb.ToString(),
                        "OK");
                }

                // CASE 2: General Server Error (e.g. 500 or 400 without validation details)
                else
                {
                    _snackbarService.Show(
                        error?.Title ?? "Error",
                        error?.Detail ?? "An unexpected error occurred.",
                        ControlAppearance.Danger,
                        new SymbolIcon(SymbolRegular.Warning24),
                        TimeSpan.FromSeconds(5));
                }
            }
            catch
            {
                // Fallback if JSON parsing fails (e.g. raw HTML 500 error)
                _snackbarService.Show("Error", $"Server returned {response.StatusCode}", ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(5));
            }
        }
    }
}
