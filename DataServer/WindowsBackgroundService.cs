
namespace DataServerService
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        private readonly Socket_For_Data_Transmission _DataServerService;
        private readonly ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService(
            Socket_For_Data_Transmission Service,
            ILogger<WindowsBackgroundService> logger) =>
            (_DataServerService, _logger) = (Service, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _DataServerService.Server_Data_Transmission();

                // Rozpocznij nas³uchiwanie na po³¹czenia
                _DataServerService.Server_Data_Transmission_Listner();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }
    }
}