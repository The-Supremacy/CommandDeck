using Mediator;
using CommandDeck.Host.Configuration;
using CommandDeck.Host.Authorization;
using CommandDeck.Host.Features.Auth;
using CommandDeck.Host.Features.CurrentUser;
using CommandDeck.Host.HostedServices;
using CommandDeck.Identity;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.CurrentUser;
using CommandDeck.Identity.Infrastructure;
using CommandDeck.Identity.Infrastructure.Persistence;
using CommandDeck.Persistence.Configuration;
using CommandDeck.Persistence.Transactions;
using CommandDeck.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddPersistence();
builder.AddHostAuthentication();
builder.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies =
    [
        typeof(ResolveCurrentUserCommand).Assembly,
        typeof(ApplicationAccessRepository).Assembly
    ];
    options.PipelineBehaviors =
    [
        typeof(CommandTransactionBehavior<,>)
    ];
});
builder.Services
    .AddOptions<InitialApplicationAccessOptions>()
    .BindConfiguration("Identity:InitialApplicationAccess");
builder.Services.AddHostedService<InitialApplicationAccessHostedService>();
builder.Services.AddIdentityModule();
builder.Services.AddIdentityInfrastructure();
builder.Services.AddApplicationAccessAuthorization();

var app = builder.Build();
app.UseProblemDetails();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapCurrentUserEndpoint();

app.Run();

public partial class Program;
