using System.Reflection;
using Agave.Silo;

var builder = Host.CreateApplicationBuilder(args);

builder.Environment.ApplicationName = "agave-silo";

builder.Services.AddApplicationMetadata(md => 
    md.BuildVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown"
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

builder.Build().Run();
