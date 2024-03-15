namespace Agave

{
    public sealed class Agave : Grain, IAgave
    {
        private readonly ILogger _logger;
        private readonly Func<Guid> _identity;

        public Agave(
            ILoggerFactory loggerFactory, 
            Func<Guid>? identity = null)
        {
            _logger = loggerFactory.CreateLogger<Agave>();
            _identity = identity ?? this.GetPrimaryKey;
        }

        Task IAgave.Plant()
        {
            _logger.LogInformation($"Planting the agave {_identity()}.");

            // this.GetStreamProvider("EventHub")
            //     .GetStream<AgavePlantedEvent>(_identity(), "AgavePlanted")
            //     .OnNextAsync(new AgavePlantedEvent(_identity()));



            return Task.CompletedTask;
        }
    }

    [Alias("Agave.IAgave")]
    public interface IAgave : IGrainWithGuidKey
    {
        [Alias("Plant")]
        Task Plant();
    }
}
