using mark.davison.rome.api.persistence.UserPreferences;

namespace mark.davison.rome.api.queries.Scenarios.GetUserPreferences;

public sealed class GetUserPreferencesQueryProcessor : IQueryProcessor<GetUserPreferencesQueryRequest, GetUserPreferencesQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;

    public GetUserPreferencesQueryProcessor(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetUserPreferencesQueryResponse> ProcessAsync(
        GetUserPreferencesQueryRequest request,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var preferences = await UserPreferencesSupport.LoadAsync(
            _dbContext,
            currentUserContext.UserId,
            cancellationToken);

        return new GetUserPreferencesQueryResponse { Value = preferences };
    }
}
