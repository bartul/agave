using Agave.Silo;

var builder = Host.CreateApplicationBuilder(args);

// builder.Services.AddLocalStorageServices();
if(builder.Environment.IsDevelopment())
{
    builder.UseOrleans((siloBuilder) =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .AddMemoryGrainStorage("agave")
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
            .AddCosmosGrainStorage("agave", cosmosOptions => cosmosOptions.ConfigureCosmosClient(connectionString));
    });
}


var host = builder.Build();
host.Run();

