namespace mark.davison.rome.api.queries.Scenarios.TagList;

public sealed class TagListQueryProcessor : IQueryProcessor<TagListQueryRequest, TagListQueryResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;

    public TagListQueryProcessor(IDbContext<RomeDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TagListQueryResponse> ProcessAsync(TagListQueryRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var tags = await _dbContext
            .Set<Tag>()
            .AsNoTracking()
            .Where(_ => _.UserId == currentUserContext.UserId)
            .ToListAsync(cancellationToken);

        return new TagListQueryResponse
        {
            Value = [.. tags.Select(t => t.ToDto())]
        };
    }
}
