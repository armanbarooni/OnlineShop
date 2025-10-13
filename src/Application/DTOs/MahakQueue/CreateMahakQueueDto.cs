namespace OnlineShop.Application.DTOs.MahakQueue
{
    public class CreateMahakQueueDto
    {
        public string QueueType { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int Priority { get; set; } = 5;
        public int MaxRetries { get; set; } = 3;
        public DateTime? ScheduledAt { get; set; }
        public string? Payload { get; set; }
    }

    public class UpdateMahakQueueDto
    {
        public Guid Id { get; set; }
        public string QueueType { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string? Payload { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class MahakQueueDto
    {
        public Guid Id { get; set; }
        public string QueueType { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public Guid? EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string QueueStatus { get; set; } = string.Empty;
        public int Priority { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public string? Payload { get; set; }
        public string? ErrorMessage { get; set; }
        public string? MahakResponse { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

