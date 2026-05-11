using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommandDeck.Persistence;
using CommandDeck.Persistence.Configuration;
using CommandDeck.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.AddPersistence();

using IHost host = builder.Build();
using IServiceScope scope = host.Services.CreateScope();

CommandDeckDbContext dbContext = scope.ServiceProvider.GetRequiredService<CommandDeckDbContext>();
await dbContext.Database.MigrateAsync();
