using Microsoft.Extensions.Options;

namespace MesEnterprise.Services.BackgroundServices
{
    /// <summary>
    /// Helper class to allow configuring BackgroundServiceOptions via Action delegates.
    /// </summary>
    public class ConfigureBackgroundServiceOptions : IConfigureOptions<BackgroundServiceOptions>
    {
        private readonly Action<BackgroundServiceOptions> _configure;
        public ConfigureBackgroundServiceOptions(Action<BackgroundServiceOptions> configure) => _configure = configure;
        public void Configure(BackgroundServiceOptions options) => _configure(options);
    }
}