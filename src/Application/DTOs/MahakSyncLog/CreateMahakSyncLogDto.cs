namespace OnlineShop.Application.DTOs.MahakSyncLog
{
    public class CreateMahakSyncLogDto
    {
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string SyncType { get; set; } = string.Empty;
        public string SyncStatus { get; set; } = string.Empty;
        public int RecordsProcessed { get; set; } = 0;
    }

    public class UpdateMahakSyncLogDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string SyncType { get; set; } = string.Empty;
        public string SyncStatus { get; set; } = string.Empty;
        public int RecordsProcessed { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class MahakSyncLogDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string SyncType { get; set; } = string.Empty;
        public string SyncStatus { get; set; } = string.Empty;
        public DateTime SyncStartedAt { get; set; }
        public DateTime? SyncCompletedAt { get; set; }
        public long DurationMs { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSuccessful { get; set; }
        public int RecordsFailed { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SyncData { get; set; }
        public string? MahakResponse { get; set; }
        public long? MahakRowVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

