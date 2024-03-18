using OrleansCodeGen.Orleans;

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

        async Task IAgave.Plant()
        {
            _logger.LogInformation($"Planting the agave {_identity()}.");
            await Task.CompletedTask;
        }
    }

    [GenerateSerializer, Immutable]
    [Alias("Agave.AgavePlanted")]
    internal record AgavePlanted(Guid AgaveId)
    {
        [Id(0)]
        public Guid AgaveId { get; init; } = AgaveId;
    }

    [Alias("Agave.IAgave")]
    public interface IAgave : IGrainWithGuidKey
    {
        [Alias("Plant")]
        Task Plant();
    }
}
