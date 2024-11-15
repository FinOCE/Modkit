﻿open Azure.Core.Serialization
open Microsoft.Azure.Functions.Worker
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open System.IO
open System.Text.Json

HostBuilder()
    .ConfigureFunctionsWorkerDefaults(fun ctx builder ->
        !builder.Services.Configure(fun (workerOptions: WorkerOptions) ->
            workerOptions.Serializer <- JsonObjectSerializer(Json.options)
        )
        !builder.ConfigureCosmosDBExtension()
    )
    .ConfigureAppConfiguration(fun builder ->
        !builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", true)
            .AddEnvironmentVariables()
    )
    .ConfigureLogging(fun logging -> ())
    .ConfigureServices(fun ctx services -> 
        !services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
    )
    .Build()
    .RunAsync()
|> Async.AwaitTask
|> Async.RunSynchronously
