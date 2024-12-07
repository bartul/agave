using Agave.Silo;

var builder = Host.CreateApplicationBuilder(args);

builder.Environment.ApplicationName = "agave-silo";

builder.Services.AddApplicationMetadata(md => 
    md.BuildVersion = builder.Environment.IsDevelopment()
                         ? "0.0.1-dev-box"
                         : typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"
);
builder.AddServiceDefaults();

builder.AddKeyedAzureTableClient("agave-clustering");
builder.AddKeyedAzureTableClient("agave-grain-state");
builder.AddKeyedAzureTableClient("agave-reminders");

builder.UseOrleans((siloBuilder) =>
{
    siloBuilder
        .AddBroadcastChannel("event-bus")
        .AddActivityPropagation()
        .AddStartupTask<GenesisSeeding>()

        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "agave-cluster";
            options.ServiceId = "agave";
        });
});

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting agave silo...");

host.Run();
