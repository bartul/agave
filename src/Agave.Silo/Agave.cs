using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using OrleansCodeGen.Orleans;

namespace Agave

{
    public sealed class Agave : Grain, IAgave
    {
        private readonly ILogger _logger;
        private readonly Func<Guid> _identity;
        private readonly IPersistentState<AgaveState> _state;

        public Agave(
            [PersistentState("agave_ecosystem_store", "agave")] IPersistentState<AgaveState> state,
            ILoggerFactory loggerFactory, 
            Func<Guid>? identity = null)
        {
            _state = state;
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
    public record AgaveState
    {
        public AgaveBlossomState Current { get; set; }
    }

    [GenerateSerializer]
    public enum AgaveBlossomState
    {
        Planted,
        Germinated,
        Blossomed,
        Dead
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
