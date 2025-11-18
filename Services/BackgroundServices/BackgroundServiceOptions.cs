namespace MesEnterprise.Services.BackgroundServices
{
    /// <summary>
    /// Provides options for configuring the behavior of background services.
    /// </summary>
    public class BackgroundServiceOptions
    {
        public TimeSpan InitialDelay { get; set; } = TimeSpan.Zero;
    }
}