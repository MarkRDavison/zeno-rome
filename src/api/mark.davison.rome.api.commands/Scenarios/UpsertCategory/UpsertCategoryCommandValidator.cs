namespace mark.davison.rome.api.commands.Scenarios.UpsertCategory;

public sealed class UpsertCategoryCommandValidator : ICommandValidator<UpsertCategoryCommandRequest, UpsertCategoryCommandResponse>
{
    public const string VALIDATION_DUPLICATE_CATEGORY = "VALIDATION_DUPLICATE_CATEGORY${0}";

    private readonly IDbContext<RomeDbContext> _dbContext;

    public UpsertCategoryCommandValidator(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpsertCategoryCommandResponse> ValidateAsync(UpsertCategoryCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var existing = await _dbContext
            .Set<Category>()
            .AsNoTracking()
            .Where(c => c.Name == request.Name && c.UserId == currentUserContext.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            if (existing.Id != request.Id)
            {
                return new UpsertCategoryCommandResponse
                {
                    Errors = [string.Format(VALIDATION_DUPLICATE_CATEGORY, request.Name)]
                };
            }
        }

        return new UpsertCategoryCommandResponse();
    }
}
