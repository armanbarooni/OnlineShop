namespace OnlineShop.Application.Common.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; private init; }
        public T Data { get; private init; }
        public string ErrorMessage { get; private init; }

        public IReadOnlyList<object>? BackGroundTasks { get; private set; }
        public IEnumerable<object>? EventMessages { get; private init; }
        public IEnumerable<object>? EventLogMessages { get; private init; }
        public IEnumerable<object>? SocketMessages { get; private init; }

        // سازنده خصوصی برای جلوگیری از ساخت مستقیم
        private Result()
        {
            ErrorMessage = string.Empty;
            Data = default(T)!;
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                ErrorMessage = string.Empty
            };
        }

        public static Result<T> Success(
            T data,
            IReadOnlyList<object>? backGroundTasks = null,
            IEnumerable<object>? eventMessages = null,
            IEnumerable<object>? eventLogMessages = null,
            IEnumerable<object>? socketMessages = null)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                BackGroundTasks = backGroundTasks,
                EventMessages = eventMessages,
                EventLogMessages = eventLogMessages,
                SocketMessages = socketMessages,
                ErrorMessage = string.Empty
            };
        }

        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                Data = default(T)!
            };
        }
    }
}