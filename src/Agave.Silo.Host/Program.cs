var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("agave-storage").RunAsEmulator();
var clusterTable = storage.AddTables("agave-clustering");
var grainTable = storage.AddTables("agave-grain-state");
var remindersTable = storage.AddTables("agave-reminders");

var orleans = builder.AddOrleans("default")
    .WithClustering(clusterTable)
    .WithGrainStorage("agave", grainTable)
    .WithReminders(remindersTable);

builder.AddProject<Projects.Agave_Silo>("agave-silo")
    .WithReference(orleans)
    .WithReplicas(3);

builder.Build().Run();
