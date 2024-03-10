namespace Agave.Silo;

internal class GenesisSeeding : IStartupTask
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<GenesisSeeding> _logger;

    public GenesisSeeding(IGrainFactory grainFactory, ILoggerFactory loggerFactory)
    {
        _grainFactory = grainFactory;
        _logger = loggerFactory.CreateLogger<GenesisSeeding>();
    }

    public Task Execute(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Seeding the initial seed.");
        return Task.CompletedTask;
    }
}

