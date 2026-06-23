namespace mark.davison.rome.api.commands.Scenarios.UpsertCategory;

public sealed class UpsertCategoryCommandProcessor : ICommandProcessor<UpsertCategoryCommandRequest, UpsertCategoryCommandResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly IDateService _dateService;

    public UpsertCategoryCommandProcessor(
        IDbContext<RomeDbContext> dbContext,
        IDateService dateService)
    {
        _dbContext = dbContext;
        _dateService = dateService;
    }

    public async Task<UpsertCategoryCommandResponse> ProcessAsync(UpsertCategoryCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var category = await _dbContext
            .GetByIdAsync<Category>(request.Id, cancellationToken);

        if (category is null)
        {
            category = new Category
            {
                Id = request.Id,
                UserId = currentUserContext.UserId,
                Name = request.Name,
                Created = _dateService.Now,
                LastModified = _dateService.Now
            };

            await _dbContext.UpsertEntityAsync(category, cancellationToken);
        }
        else
        {
            category.Name = request.Name;
            category.LastModified = _dateService.Now;

            await _dbContext.UpsertEntityAsync(category, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(true, cancellationToken);

        return new UpsertCategoryCommandResponse
        {
            Value = category.ToDto()
        };
    }
}
