using Agave.Silo
var builder = Host.CreateApplicationBuilder(args);

builder.Environment.ApplicationName = builder.Configuration.GetValue("ServiceName", defaultValue: nameof(Agave))!;
var applicationVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";

var tracingExporter = builder.Configuration.GetValue("UseTracingExporter", defaultValue: "console")!.ToLowerInvariant();
var metricsExporter = builder.Configuration.GetValue("UseMetricsExporter", defaultValue: "console")!.ToLowerInvariant();
var logExporter = builder.Configuration.GetValue("UseLogExporter", defaultValue: "console")!.ToLowerInvariant();

builder.SetupOrleans();
builder.SetupTelemetry(applicationVersion, tracingExporter, metricsExporter, logExporter);

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Agave Silo");

host.Run();
