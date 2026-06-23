namespace mark.davison.rome.api.queries.Scenarios.CategoryList;

public sealed class CategoryListQueryProcessor : IQueryProcessor<CategoryListQueryRequest, CategoryListQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;

    public CategoryListQueryProcessor(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CategoryListQueryResponse> ProcessAsync(CategoryListQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var categories = await _dbContext
            .Set<Category>()
            .AsNoTracking()
            .Where(_ => _.UserId == currentUserContext.UserId)
            .ToListAsync(cancellationToken);

        return new CategoryListQueryResponse
        {
            Value = [.. categories.Select(c => c.ToDto())]
        };
    }
}
