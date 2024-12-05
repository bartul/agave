var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("agave").RunAsEmulator();

var orleans = builder.AddOrleans("default").WithDevelopmentClustering();

builder.AddProject<Projects.Agave_Silo>("silo")
    .WithReference(orleans)
    .WithReplicas(1);

builder.Build().Run();
