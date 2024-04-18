namespace Agave.Silo;

internal class GenesisSeeding(IGrainFactory grainFactory, ILoggerFactory loggerFactory) : IStartupTask
{
    private readonly IGrainFactory _grainFactory = grainFactory;
    private readonly ILogger<GenesisSeeding> _logger = loggerFactory.CreateLogger<GenesisSeeding>();

    public async Task Execute(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding the initial seed.");
        await _grainFactory.GetGrain<IAgave>(Guid.NewGuid()).Plant(new PlantSeedCommand(
            TimeToGerminate: TimeSpan.FromSeconds(5), 
            SuccessRate: 1, 
            DegenerationRate: 0.1,
            TimeToBlossom: TimeSpan.FromSeconds(5),
            NumberOfSeedsProducing: 3));
    }
}

