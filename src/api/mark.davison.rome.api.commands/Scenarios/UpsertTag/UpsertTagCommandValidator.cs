namespace mark.davison.rome.api.commands.Scenarios.UpsertTag;

public sealed class UpsertTagCommandValidator : ICommandValidator<UpsertTagCommandRequest, UpsertTagCommandResponse>
{
    public const string VALIDATION_DUPLICATE_TAG = "VALIDATION_DUPLICATE_TAG${0}";

    private readonly IDbContext<RomeDbContext> _dbContext;

    public UpsertTagCommandValidator(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpsertTagCommandResponse> ValidateAsync(UpsertTagCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var existing = await _dbContext
            .Set<Tag>()
            .AsNoTracking()
            .Where(t => t.Name == request.Name && t.UserId == currentUserContext.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            if (existing.Id != request.Id)
            {
                return new UpsertTagCommandResponse
                {
                    Errors = [string.Format(VALIDATION_DUPLICATE_TAG, request.Name)]
                };
            }
        }

        return new UpsertTagCommandResponse();
    }
}
