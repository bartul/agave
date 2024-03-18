using Agave.Silo;

var builder = Host.CreateApplicationBuilder(args);

if(builder.Environment.IsDevelopment())
{
    builder.UseOrleans((siloBuilder) =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .AddMemoryGrainStorage("agave")
            .AddMemoryGrainStorage("ActorEventHub")
            .AddMemoryStreams("ActorEventHub")

            .AddStartupTask<GenesisSeeding>();
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
            .AddCosmosGrainStorage("agave_ecosystem_store", cosmosOptions => cosmosOptions.ConfigureCosmosClient(connectionString))
            .AddEventHubStreams("ActorEventHub", options =>
            {
                // options.ConnectionOptions.ConnectionString = builder.Configuration.GetValue<string>("ORLEANS_AZURE_EVENT_HUB_CONNECTION_STRING") ?? "";
                // options.ConsumerGroup = builder.Configuration.GetValue<string>("ORLEANS_AZURE_EVENT_HUB_CONSUMER_GROUP") ?? "";
                // options.Path = builder.Configuration.GetValue<string>("ORLEANS_AZURE_EVENT_HUB_PATH") ?? "";
            });
    });
}


var host = builder.Build();
host.Run();

