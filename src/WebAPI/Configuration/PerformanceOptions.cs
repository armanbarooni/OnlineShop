namespace OnlineShop.WebAPI.Configuration
{
    public class PerformanceOptions
    {
        public bool ApplyMigrationsOnStartup { get; set; }
        public int SlowRequestThresholdMs { get; set; } = 800;
        public bool LogStaticFileRequests { get; set; } = false;
        public bool EnableKeepAlivePing { get; set; } = true;
        public int KeepAliveIntervalSeconds { get; set; } = 60;
        public string KeepAlivePath { get; set; } = "/api/health";
    }
}
