namespace SocialNetworkApp.Server.Business.Services.Redis
{
    public class MessageQueueWorker(ICacheService cacheService) : BackgroundService
    {
        private readonly ICacheService _cacheService = cacheService;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _cacheService.DequeueMessageAsync("messageQueue");
                if (!string.IsNullOrEmpty(message))
                {
                    //jedino mozda za indeksiranje kroz neo4j, mora se doda u repo metoda koja
                    //vraca poruku po id-u pa je indeksira
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }
}
