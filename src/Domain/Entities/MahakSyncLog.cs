using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class MahakSyncLog : BaseEntity
    {
        public string EntityType { get; private set; } = string.Empty;
        public Guid? EntityId { get; private set; }
        public string SyncType { get; private set; } = string.Empty; // "Import", "Export", "Update"
        public string SyncStatus { get; private set; } = string.Empty; // "Success", "Failed", "Pending"
        public DateTime SyncStartedAt { get; private set; }
        public DateTime? SyncCompletedAt { get; private set; }
        public long DurationMs { get; private set; }
        public int RecordsProcessed { get; private set; }
        public int RecordsSuccessful { get; private set; }
        public int RecordsFailed { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? SyncData { get; private set; } // JSON data for debugging
        public string? MahakResponse { get; private set; }
        public long? MahakRowVersion { get; private set; }

        protected MahakSyncLog() { }

        private MahakSyncLog(string entityType, Guid? entityId, string syncType, string syncStatus, 
            DateTime syncStartedAt, int recordsProcessed)
        {
            SetEntityType(entityType);
            EntityId = entityId;
            SetSyncType(syncType);
            SetSyncStatus(syncStatus);
            SyncStartedAt = syncStartedAt;
            SetRecordsProcessed(recordsProcessed);
            RecordsSuccessful = 0;
            RecordsFailed = 0;
            DurationMs = 0;
            Deleted = false;
        }

        public static MahakSyncLog Create(string entityType, Guid? entityId, string syncType, 
            string syncStatus, int recordsProcessed = 0)
            => new(entityType, entityId, syncType, syncStatus, DateTime.UtcNow, recordsProcessed);

        public void SetEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("نوع موجودیت نباید خالی باشد");
            EntityType = entityType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSyncType(string syncType)
        {
            if (string.IsNullOrWhiteSpace(syncType))
                throw new ArgumentException("نوع سینک نباید خالی باشد");
            SyncType = syncType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSyncStatus(string syncStatus)
        {
            if (string.IsNullOrWhiteSpace(syncStatus))
                throw new ArgumentException("وضعیت سینک نباید خالی باشد");
            SyncStatus = syncStatus.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRecordsProcessed(int recordsProcessed)
        {
            if (recordsProcessed < 0)
                throw new ArgumentException("تعداد رکوردهای پردازش شده نمی‌تواند منفی باشد");
            RecordsProcessed = recordsProcessed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRecordsSuccessful(int recordsSuccessful)
        {
            if (recordsSuccessful < 0)
                throw new ArgumentException("تعداد رکوردهای موفق نمی‌تواند منفی باشد");
            RecordsSuccessful = recordsSuccessful;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetRecordsFailed(int recordsFailed)
        {
            if (recordsFailed < 0)
                throw new ArgumentException("تعداد رکوردهای ناموفق نمی‌تواند منفی باشد");
            RecordsFailed = recordsFailed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetErrorMessage(string? errorMessage)
        {
            ErrorMessage = errorMessage?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetSyncData(string? syncData)
        {
            SyncData = syncData?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMahakResponse(string? mahakResponse)
        {
            MahakResponse = mahakResponse?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMahakRowVersion(long? mahakRowVersion)
        {
            MahakRowVersion = mahakRowVersion;
            UpdatedAt = DateTime.UtcNow;
        }

        public void CompleteSync(int recordsSuccessful, int recordsFailed, string? errorMessage = null)
        {
            SyncCompletedAt = DateTime.UtcNow;
            DurationMs = (long)(SyncCompletedAt.Value - SyncStartedAt).TotalMilliseconds;
            SetRecordsSuccessful(recordsSuccessful);
            SetRecordsFailed(recordsFailed);
            SetErrorMessage(errorMessage);
            
            SyncStatus = recordsFailed == 0 ? "Success" : "Partial";
            UpdatedAt = DateTime.UtcNow;
        }

        public void FailSync(string errorMessage)
        {
            SyncCompletedAt = DateTime.UtcNow;
            DurationMs = (long)(SyncCompletedAt.Value - SyncStartedAt).TotalMilliseconds;
            SetErrorMessage(errorMessage);
            SyncStatus = "Failed";
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(string entityType, string syncType, string syncStatus, 
            int recordsProcessed, string? updatedBy)
        {
            SetEntityType(entityType);
            SetSyncType(syncType);
            SetSyncStatus(syncStatus);
            SetRecordsProcessed(recordsProcessed);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این لاگ سینک قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
