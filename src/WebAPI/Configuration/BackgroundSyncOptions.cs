namespace OnlineShop.WebAPI.Configuration
{
    public class BackgroundSyncOptions
    {
        public bool Enabled { get; set; } = true;
        public int IncomingInitialDelaySeconds { get; set; } = 20;
        public int IncomingIntervalMinutes { get; set; } = 5;
        public int OutgoingInitialDelaySeconds { get; set; } = 45;
        public int OutgoingIntervalMinutes { get; set; } = 1;
    }
}
