using OnlineShop.Domain.Common;

namespace OnlineShop.Domain.Entities
{
    public class MahakQueue : BaseEntity
    {
        public string QueueType { get; private set; } = string.Empty; // "Order", "Payment", "Inventory", "Product"
        public string OperationType { get; private set; } = string.Empty; // "Create", "Update", "Delete"
        public Guid? EntityId { get; private set; }
        public string EntityType { get; private set; } = string.Empty;
        public string QueueStatus { get; private set; } = "Pending"; // "Pending", "Processing", "Completed", "Failed", "Retry"
        public int Priority { get; private set; }
        public int RetryCount { get; private set; }
        public int MaxRetries { get; private set; }
        public DateTime? ScheduledAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public DateTime? FailedAt { get; private set; }
        public string? Payload { get; private set; } // JSON data
        public string? ErrorMessage { get; private set; }
        public string? MahakResponse { get; private set; }
        public DateTime? NextRetryAt { get; private set; }

        protected MahakQueue() { }

        private MahakQueue(string queueType, string operationType, Guid? entityId, string entityType, 
            int priority, int maxRetries, DateTime? scheduledAt, string? payload)
        {
            SetQueueType(queueType);
            SetOperationType(operationType);
            EntityId = entityId;
            SetEntityType(entityType);
            SetPriority(priority);
            SetMaxRetries(maxRetries);
            SetScheduledAt(scheduledAt);
            SetPayload(payload);
            QueueStatus = "Pending";
            RetryCount = 0;
            Deleted = false;
        }

        public static MahakQueue Create(string queueType, string operationType, Guid? entityId, 
            string entityType, int priority = 5, int maxRetries = 3, DateTime? scheduledAt = null, 
            string? payload = null)
            => new(queueType, operationType, entityId, entityType, priority, maxRetries, scheduledAt, payload);

        public void SetQueueType(string queueType)
        {
            if (string.IsNullOrWhiteSpace(queueType))
                throw new ArgumentException("نوع صف نباید خالی باشد");
            QueueType = queueType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetOperationType(string operationType)
        {
            if (string.IsNullOrWhiteSpace(operationType))
                throw new ArgumentException("نوع عملیات نباید خالی باشد");
            OperationType = operationType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEntityType(string entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
                throw new ArgumentException("نوع موجودیت نباید خالی باشد");
            EntityType = entityType.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPriority(int priority)
        {
            if (priority < 1 || priority > 10)
                throw new ArgumentException("اولویت باید بین 1 تا 10 باشد");
            Priority = priority;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMaxRetries(int maxRetries)
        {
            if (maxRetries < 0)
                throw new ArgumentException("حداکثر تعداد تلاش نمی‌تواند منفی باشد");
            MaxRetries = maxRetries;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetScheduledAt(DateTime? scheduledAt)
        {
            ScheduledAt = scheduledAt;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPayload(string? payload)
        {
            Payload = payload?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetErrorMessage(string? errorMessage)
        {
            ErrorMessage = errorMessage?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetMahakResponse(string? mahakResponse)
        {
            MahakResponse = mahakResponse?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetNextRetryAt(DateTime? nextRetryAt)
        {
            NextRetryAt = nextRetryAt;
            UpdatedAt = DateTime.UtcNow;
        }

        public void StartProcessing()
        {
            QueueStatus = "Processing";
            UpdatedAt = DateTime.UtcNow;
        }

        public void CompleteProcessing(string? mahakResponse)
        {
            QueueStatus = "Completed";
            ProcessedAt = DateTime.UtcNow;
            SetMahakResponse(mahakResponse);
            UpdatedAt = DateTime.UtcNow;
        }

        public void FailProcessing(string errorMessage)
        {
            QueueStatus = "Failed";
            FailedAt = DateTime.UtcNow;
            SetErrorMessage(errorMessage);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ScheduleRetry(DateTime retryAt)
        {
            if (RetryCount >= MaxRetries)
            {
                QueueStatus = "Failed";
                FailedAt = DateTime.UtcNow;
            }
            else
            {
                QueueStatus = "Retry";
                RetryCount++;
                SetNextRetryAt(retryAt);
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void ResetForRetry()
        {
            QueueStatus = "Pending";
            SetNextRetryAt(null);
            UpdatedAt = DateTime.UtcNow;
        }

        public bool CanRetry() => RetryCount < MaxRetries;

        public void Update(string queueType, string operationType, string entityType, 
            int priority, string? payload, string? updatedBy)
        {
            SetQueueType(queueType);
            SetOperationType(operationType);
            SetEntityType(entityType);
            SetPriority(priority);
            SetPayload(payload);
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete(string? updatedBy)
        {
            if (Deleted)
                throw new InvalidOperationException("این آیتم صف قبلاً حذف شده است.");
            Deleted = true;
            UpdatedBy = updatedBy;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
