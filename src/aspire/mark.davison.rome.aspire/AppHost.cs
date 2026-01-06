using mark.davison.rome.aspire;
using mark.davison.rome.shared.constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Projects;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.Development.json")
    .Build();

var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter("dbusername", "romedbuser", secret: false);
var dbPassword = builder.AddParameter("dbpassword", "romedbpassword", secret: true);


var redis = builder
    .AddRedis(AspireConstants.Redis, 6380)
    .WithoutHttpsCertificate();

var db = builder
    .AddPostgres(AspireConstants.Postgres, dbUsername, dbPassword, port: AspireConstants.PostgresPort)
    .AddDatabase(AspireConstants.PostgresDatabase, AspireConstants.PostgresDatabase);

//.AddDockerfile()

var api = builder
    .AddProject<mark_davison_rome_api>(AspireConstants.Api)
    .WithConfig(config)
    .WithNonProxiedHttpsEndpoint()
    .WithCommonHealthChecks()
    .WithExternalHttpEndpoints()
    .WithPostgresDatabase(dbUsername, dbPassword)
    .WithReference(db)
    .WaitFor(db)
    .WaitFor(redis);

var bff = builder
    .AddProject<mark_davison_rome_bff>(AspireConstants.Bff)
    .WithConfig(config)
    .WithNonProxiedHttpsEndpoint()
    .WithCommonHealthChecks()
    .WithExternalHttpEndpoints()
    .WithRedis(redis)
    .WithReference(api)
    .WaitFor(redis)
    .WaitFor(db)
    .WaitFor(api);

builder
    .AddProject<mark_davison_rome_web>(AspireConstants.Web)
    .WithNonProxiedHttpsEndpoint()
    .WithExternalHttpEndpoints()
    .WithReference(bff)
    .WaitFor(bff);

builder.Build().Run();
