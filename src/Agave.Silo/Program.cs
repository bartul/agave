using Agave.Silo;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

var builder = Host.CreateApplicationBuilder(args);
builder.Environment.ApplicationName = nameof(Agave);
var applicationVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";

builder.Services
    .AddApplicationMetadata(md =>
    {
        md.ApplicationName = builder.Environment.ApplicationName;
        md.BuildVersion = applicationVersion;
        md.EnvironmentName = builder.Environment.EnvironmentName;
    })
    .AddProcessLogEnricher(e => e.ProcessId = true)
    .AddServiceLogEnricher(e =>
    {
        e.ApplicationName = true;
        e.BuildVersion = true;
        e.EnvironmentName = true;
    });
builder.Logging
    .EnableEnrichment()
    .AddJsonConsole(o => o.JsonWriterOptions = new System.Text.Json.JsonWriterOptions() { Indented = true });

if (builder.Environment.IsDevelopment())
{
    builder.UseOrleans((siloBuilder) =>
    {
        siloBuilder
            .UseLocalhostClustering()
            .AddMemoryGrainStorage("agave")
            .UseInMemoryReminderService()
            .AddStartupTask<GenesisSeeding>();
    });
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(
            serviceName: builder.Environment.ApplicationName,
            serviceVersion: applicationVersion,
            serviceInstanceId: Environment.MachineName))
        .WithMetrics(metricBuilder =>
        {
            metricBuilder
                .AddMeter("Microsoft.Orleans")
                .AddHttpClientInstrumentation()
                .AddConsoleExporter();
        })
        .WithTracing(tracingBuilder =>
        {
            tracingBuilder
                .AddHttpClientInstrumentation()                
                .AddSource("Microsoft.Orleans.Runtime")
                .AddSource("Microsoft.Orleans.Application")
                .AddConsoleExporter();
        });
    builder.Logging.ClearProviders();
    builder.Logging.AddOpenTelemetry(logging =>
    {
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(builder.Environment.ApplicationName);
        logging.SetResourceBuilder(resourceBuilder)
            .AddConsoleExporter();
    });
    builder.Services.Configure<OpenTelemetryLoggerOptions>(options => options.IncludeScopes = true);
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

