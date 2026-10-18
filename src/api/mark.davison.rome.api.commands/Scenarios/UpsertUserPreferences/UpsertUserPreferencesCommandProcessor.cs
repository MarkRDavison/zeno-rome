using mark.davison.rome.api.persistence.UserPreferences;

namespace mark.davison.rome.api.commands.Scenarios.UpsertUserPreferences;

public sealed class UpsertUserPreferencesCommandProcessor : ICommandProcessor<UpsertUserPreferencesCommandRequest, UpsertUserPreferencesCommandResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    public UpsertUserPreferencesCommandProcessor(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpsertUserPreferencesCommandResponse> ProcessAsync(
        UpsertUserPreferencesCommandRequest request,
        ICurrentUserContext currentUserContext,
        CancellationToken cancellationToken)
    {
        var normalized = UserPreferencesSupport.Normalize(request.Preferences);
        var json = UserPreferencesSupport.Serialize(normalized);

        var existing = await _dbContext
            .Set<UserPreference>()
            .FirstOrDefaultAsync(_ => _.UserId == currentUserContext.UserId, cancellationToken);

        if (existing is null)
        {
            _dbContext.Set<UserPreference>().Add(new UserPreference
            {
                UserId = currentUserContext.UserId,
                PreferencesJson = json
            });
        }
        else
        {
            existing.PreferencesJson = json;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpsertUserPreferencesCommandResponse { Value = normalized };
    }
}
