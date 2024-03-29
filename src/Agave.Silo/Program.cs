using Agave.Silo;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

if(builder.Environment.IsDevelopment())
{

    builder.UseOrleans((siloBuilder) =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .AddMemoryGrainStorage("agave")
            .AddMemoryGrainStorage("ActorEventHub")
            .UseInMemoryReminderService()

            .AddStartupTask<GenesisSeeding>();
    });
    builder.Logging
        .EnableEnrichment()
        .AddJsonConsole(o => o.JsonWriterOptions = new System.Text.Json.JsonWriterOptions() { Indented = true });
    builder.Services
        .AddApplicationMetadata(md =>
        {
            md.ApplicationName = nameof(Agave);
            md.BuildVersion = "1.0.0";
            md.EnvironmentName = builder.Environment.EnvironmentName;
        
        })
        .AddProcessLogEnricher(e => e.ProcessId = true)
        .AddServiceLogEnricher(e =>
        {
            e.ApplicationName = true;
            e.BuildVersion = true;
            e.EnvironmentName = true;
        });

}
else
{
    builder.UseOrleans(siloBuilder =>
    {
        var connectionString = builder.Configuration.GetValue<string>("ORLEANS_AZURE_COSMOS_DB_CONNECTION_STRING") ?? "";

        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "AgaveCuster";
            options.ServiceId = "Agave";
        });

        siloBuilder
            .UseCosmosClustering(cosmosOptions => cosmosOptions.ConfigureCosmosClient(connectionString))
            .AddCosmosGrainStorage("agave_ecosystem_store", cosmosOptions => cosmosOptions.ConfigureCosmosClient(connectionString));

    });
}


var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Agave Silo");
host.Run();

