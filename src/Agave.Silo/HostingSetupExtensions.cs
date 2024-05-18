using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

namespace Agave.Silo;

public static class HostingSetupExtensions
{
    public static HostApplicationBuilder SetupOrleans(this HostApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.UseOrleans((siloBuilder) =>
            {
                siloBuilder
                    .UseLocalhostClustering()
                    .AddMemoryGrainStorage("agave")
                    .UseInMemoryReminderService()
                    .AddMemoryStreams("bus")
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddBroadcastChannel("event-bus")
                    .AddActivityPropagation()
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
                    .AddCosmosGrainStorage("agave_ecosystem_store", cosmosOptions => cosmosOptions.ConfigureCosmosClient(connectionString));

                siloBuilder.AddActivityPropagation();
            });

        }
        return builder;
    }

    public static HostApplicationBuilder SetupTelemetry(this HostApplicationBuilder builder, string applicationVersion, string tracingExporter, string metricsExporter, string logExporter)
    {
        builder.Services
            .AddApplicationMetadata(md =>
            {
                md.ApplicationName = builder.Environment.ApplicationName;
                md.BuildVersion = applicationVersion;
                md.EnvironmentName = builder.Environment.EnvironmentName;
            })
            .AddProcessLogEnricher(e =>
            {
                e.ProcessId = true;
                e.ThreadId = true;
            })
            .AddServiceLogEnricher(e =>
            {
                e.ApplicationName = true;
                e.BuildVersion = true;
                e.EnvironmentName = true;
            });
        builder.Logging
            .ClearProviders()
            .EnableEnrichment();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: builder.Environment.ApplicationName,
                serviceNamespace: "agave",
                serviceVersion: applicationVersion,
                serviceInstanceId: Environment.MachineName))
            .WithMetrics(metricBuilder =>
            {
                metricBuilder
                    .AddMeter("Microsoft.Orleans")
                    .AddHttpClientInstrumentation()
                    .AddTelemetryExporters(metricsExporter);
            })
            .WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .AddHttpClientInstrumentation()
                    .AddSource("Microsoft.Orleans.Runtime")
                    .AddSource("Microsoft.Orleans.Application")
                    .AddTelemetryExporters(tracingExporter);
            });
        builder.Logging.AddOpenTelemetry(logging =>
        {
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(builder.Environment.ApplicationName);
            logging.SetResourceBuilder(resourceBuilder)
                .AddTelemetryExporters(logExporter);
        });

        return builder;
    }

    public static TracerProviderBuilder AddTelemetryExporters(this TracerProviderBuilder builder, string tracingExporter) => tracingExporter switch
    {
        "otlp" => builder.AddOtlpExporter(),
        "console" => builder.AddConsoleExporter(),
        _ => builder
    };

    public static MeterProviderBuilder AddTelemetryExporters(this MeterProviderBuilder builder, string metricsExporter) => metricsExporter switch
    {
        "otlp" => builder.AddOtlpExporter(),
        "console" => builder.AddConsoleExporter(),
        _ => builder
    };

    public static OpenTelemetryLoggerOptions AddTelemetryExporters(this OpenTelemetryLoggerOptions builder, string logExporter) => logExporter switch
    {
        "otlp" => builder.AddOtlpExporter(),
        "console" => builder.AddConsoleExporter(),
        _ => builder
    };
}