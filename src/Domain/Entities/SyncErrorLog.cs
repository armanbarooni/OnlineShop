using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class SyncErrorLog : BaseEntity
    {
        public string ErrorType { get; private set; } = string.Empty; // "Sync", "Validation", "Network", "Timeout"
        public string EntityType { get; private set; } = string.Empty;
        public Guid? EntityId { get; private set; }
        public string ErrorCode { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;
        public string? StackTrace { get; private set; }
        public string? RequestData { get; private set; } // JSON data
        public string? ResponseData { get; private set; } // JSON data
        public string ErrorSeverity { get; private set; } = "Medium"; // "Low", "Medium", "High", "Critical"
        public bool IsResolved { get; private set; }
        public DateTime? ResolvedAt { get; private set; }
        public string? ResolvedBy { get; private set; }
        public string? ResolutionNotes { get; private set; }
        public int OccurrenceCount { get; private set; }
        public DateTime LastOccurredAt { get; private set; }

        protected SyncErrorLog() { }

        private SyncErrorLog(string errorType, string entityType, Guid? entityId, string errorCode, 
            string errorMessage, string errorSeverity, string? requestData, string? responseData)
        {
            SetErrorType(errorType);
            SetEntityType(entityType);
            EntityId = entityId;
            SetErrorCode(errorCode);
            SetErrorMessage(errorMessage);
            SetErrorSeverity(errorSeverity);
            SetRequestData(requestData);
            SetResponseData(responseData);
            IsResolved = false;
            OccurrenceCount = 1;
            LastOccurredAt = DateTime.UtcNow;
            Deleted = false;
        }

        public static SyncErrorLog Create(string errorType, string entityType, Guid? entityId, 
            string errorCode, string errorMessage, string errorSeverity = "Medium", 
            string? requestData = null, string? responseData = null)
            => new(errorType, entityType, entityId, errorCode, errorMessage, errorSeverity, requestData, responseData);

        public void SetErrorType(string errorType)
        {
            if (string.IsNullOrWhiteSpace(errorType))
                throw new ArgumentException("نوع خطا نباید خالی باشد");
            ErrorType = errorType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("نوع موجودیت نباید خالی باشد");
            EntityType = entityType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetErrorCode(string errorCode)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
                throw new ArgumentException("کد خطا نباید خالی باشد");
            ErrorCode = errorCode.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetErrorMessage(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw new ArgumentException("پیام خطا نباید خالی باشد");
            ErrorMessage = errorMessage.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetStackTrace(string? stackTrace)
        {
            StackTrace = stackTrace?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRequestData(string? requestData)
        {
            RequestData = requestData?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetResponseData(string? responseData)
        {
            ResponseData = responseData?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetErrorSeverity(string errorSeverity)
        {
            if (string.IsNullOrWhiteSpace(errorSeverity))
                throw new ArgumentException("شدت خطا نباید خالی باشد");
            ErrorSeverity = errorSeverity.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetResolutionNotes(string? resolutionNotes)
        {
            ResolutionNotes = resolutionNotes?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncrementOccurrence()
        {
            OccurrenceCount++;
            LastOccurredAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Resolve(string resolvedBy, string? resolutionNotes)
        {
            IsResolved = true;
            ResolvedAt = DateTime.UtcNow;
            ResolvedBy = resolvedBy;
            SetResolutionNotes(resolutionNotes);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Unresolve()
        {
            IsResolved = false;
            ResolvedAt = null;
            ResolvedBy = null;
            ResolutionNotes = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string errorType, string entityType, string errorCode, 
            string errorMessage, string errorSeverity, string? updatedBy)
        {
            SetErrorType(errorType);
            SetEntityType(entityType);
            SetErrorCode(errorCode);
            SetErrorMessage(errorMessage);
            SetErrorSeverity(errorSeverity);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این لاگ خطا قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
