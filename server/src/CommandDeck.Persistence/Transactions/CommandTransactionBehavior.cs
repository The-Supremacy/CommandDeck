using Mediator;

namespace CommandDeck.Persistence.Transactions;

public sealed class CommandTransactionBehavior<TCommand, TResponse>(
    CommandDeckDbContext dbContext) : IPipelineBehavior<TCommand, TResponse>
    where TCommand : IBaseCommand
{
    public async ValueTask<TResponse> Handle(
        TCommand message,
        MessageHandlerDelegate<TCommand, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is not null)
        {
            TResponse nestedResponse = await next(message, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return nestedResponse;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        TResponse response = await next(message, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return response;
    }
}
