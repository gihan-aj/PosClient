namespace PosClient.Desktop.Shared
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public string? ErrorMessage { get; }

        private Result(bool isSuccess, T? data, string? errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T data) => new(true, data, null);
        public static Result<T> Failure(string errorMessage) => new(false, default, errorMessage);
    }

    // A helper for void-returning methods (like DELETE or POST without response body)
    public class Result
    {
        public bool IsSuccess { get; }
        public string? ErrorMessage { get; }

        private Result(bool isSuccess, string? errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(string errorMessage) => new(false, errorMessage);
    }

    public static class ResultExtensions
    {
        public static Result ToResult<T>(this Result<T> genericResult)
        {
            return Result.Failure(genericResult.ErrorMessage ?? "Unknown Error");
        }
    }
}
