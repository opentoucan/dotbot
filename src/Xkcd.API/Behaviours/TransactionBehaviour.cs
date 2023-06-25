using MediatR;
using Xkcd.API.Extensions;
using Xkcd.API.Infrastructure;

namespace Xkcd.API.Behaviours;

public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehaviour<TRequest, TResponse>> _logger;
    private readonly DbContext _dbContext;
    

    public TransactionBehaviour(ILogger<TransactionBehaviour<TRequest, TResponse>> logger, DbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = default(TResponse);
        var typeName = request.GetGenericTypeName();

        try
        {
            if (_dbContext.TransactionId != null)
                return await next();
            
            await _dbContext.BeginTransactionAsync();
            
            using (_logger.BeginScope(new List<KeyValuePair<string, object>> { new ("TransactionContext", _dbContext.TransactionId) }))
            {
                _logger.LogInformation("Begin transaction {TransactionId} for {CommandName} ({@Command})", _dbContext.TransactionId, typeName, request);

                response = await next();

                _logger.LogInformation("Commit transaction {TransactionId} for {CommandName}", _dbContext.TransactionId, typeName);

                await _dbContext.CommitTransactionAsync();
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Handling transaction for {CommandName} ({@Command})", typeName, request);

            throw;
        }
    }
}