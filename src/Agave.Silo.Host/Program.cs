var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("agave-storage").RunAsEmulator();
var clusterTable = storage.AddTables("agave-clustering");
var grainStorage = storage.AddBlobs("agave-grain-state");

var orleans = builder.AddOrleans("default").WithDevelopmentClustering();

builder.AddProject<Projects.Agave_Silo>("agave-silo")
    .WithReference(orleans)
    .WithReplicas(1);

builder.Build().Run();
