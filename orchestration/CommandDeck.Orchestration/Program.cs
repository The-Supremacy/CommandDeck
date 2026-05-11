var builder = DistributedApplication.CreateBuilder(args);

var postgresUsername = builder.AddParameter("postgres-username");
var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", postgresUsername, postgresPassword, port: 5432)
    .WithDataVolume("postgres-data")
    .WithPgAdmin();
var database = postgres.AddDatabase("commanddeck-host", "commanddeck");

var sessionTickets = builder.AddRedis("session-tickets", port: 6379)
    .WithDataVolume("redis-session-data");

var keycloakUsername = builder.AddParameter("keycloak-username");
var keycloakPassword = builder.AddParameter("keycloak-password", secret: true);
var keycloak = builder.AddKeycloak("keycloak", 8080, keycloakUsername, keycloakPassword)
    .WithDataVolume("keycloak-data")
    .WithRealmImport("./Realms");

var migrator = builder.AddProject<Projects.CommandDeck_Migrator>("migrator")
    .WithReference(database)
    .WaitFor(database);

var host = builder.AddProject<Projects.CommandDeck_Host>("host")
    .WithExternalHttpEndpoints()
    .WithReference(database)
    .WithReference(sessionTickets)
    .WithEnvironment("Authentication__Oidc__Authority", "http://localhost:8080/realms/commanddeck")
    .WithEnvironment("Authentication__Oidc__ClientId", "commanddeck-host")
    .WithEnvironment("Authentication__Oidc__CallbackPath", "/auth/callback")
    .WithEnvironment("Authentication__Oidc__SignedOutCallbackPath", "/auth/signed-out")
    .WithEnvironment("Authentication__Oidc__RequireHttpsMetadata", "false")
    .WithEnvironment("Identity__InitialApplicationAccess__Provider", "http://localhost:8080/realms/commanddeck")
    .WithEnvironment("Identity__InitialApplicationAccess__Subject", "00000000-0000-0000-0000-000000000001")
    .WaitFor(database)
    .WaitFor(sessionTickets)
    .WaitFor(keycloak)
    .WaitForCompletion(migrator);

builder.AddViteApp("admin", "../../web/apps/admin")
    .WithEndpoint("http", endpoint => endpoint.Port = 5173)
    .WithPnpm()
    .WithReference(host)
    .WithEnvironment("VITE_HOST_ORIGIN", host.GetEndpoint("http"))
    .WaitFor(host);

builder.AddViteApp("web", "../../web/apps/web")
    .WithEndpoint("http", endpoint => endpoint.Port = 5174)
    .WithPnpm()
    .WithReference(host)
    .WithEnvironment("VITE_HOST_ORIGIN", host.GetEndpoint("http"))
    .WaitFor(host);

builder.Build().Run();
