using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Infrastructure;
using CommandDeck.Migrator;
using CommandDeck.Persistence.Configuration;
using CommandDeck.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddPersistence();
builder.Services
    .AddOptions<InitialAdminOptions>()
    .BindConfiguration("Identity:InitialAdmin");
builder.Services.AddIdentityInfrastructure();
builder.Services.AddScoped<MigratorRunner>();

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();
var runner = scope.ServiceProvider.GetRequiredService<MigratorRunner>();
return await runner.RunAsync(args, CancellationToken.None);
