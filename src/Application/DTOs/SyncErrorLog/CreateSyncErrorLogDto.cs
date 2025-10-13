namespace OnlineShop.Application.DTOs.SyncErrorLog
{
    public class CreateSyncErrorLogDto
    {
        public string ErrorType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorSeverity { get; set; } = "Medium";
        public string? RequestData { get; set; }
        public string? ResponseData { get; set; }
    }

    public class UpdateSyncErrorLogDto
    {
        public Guid Id { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorSeverity { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }

    public class SyncErrorLogDto
    {
        public Guid Id { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? RequestData { get; set; }
        public string? ResponseData { get; set; }
        public string ErrorSeverity { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? ResolutionNotes { get; set; }
        public int OccurrenceCount { get; set; }
        public DateTime LastOccurredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

