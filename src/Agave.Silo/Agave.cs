namespace Agave

{
    internal sealed class Agave(ILoggerFactory loggerFactory) : Grain, IAgave
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<Agave>();

        Task IAgave.Plant()
        {
            _logger.LogInformation("Planting the agave.");
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
