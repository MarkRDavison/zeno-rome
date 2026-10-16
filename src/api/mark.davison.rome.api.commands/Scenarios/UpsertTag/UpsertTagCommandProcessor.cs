namespace mark.davison.rome.api.commands.Scenarios.UpsertTag;

public sealed class UpsertTagCommandProcessor : ICommandProcessor<UpsertTagCommandRequest, UpsertTagCommandResponse>
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly IDateService _dateService;

    public UpsertTagCommandProcessor(
        IDbContext<RomeDbContext> dbContext,
        IDateService dateService)
    {
        _dbContext = dbContext;
        _dateService = dateService;
    }

    public async Task<UpsertTagCommandResponse> ProcessAsync(UpsertTagCommandRequest request, ICurrentUserContext currentUserContext, CancellationToken cancellationToken)
    {
        var tag = await _dbContext
            .GetByIdAsync<Tag>(request.Id, cancellationToken);

        if (tag is null)
        {
            tag = new Tag
            {
                Id = request.Id,
                UserId = currentUserContext.UserId,
                Name = request.Name,
                Created = _dateService.Now,
                LastModified = _dateService.Now
            };

            await _dbContext.UpsertEntityAsync(tag, cancellationToken);
        }
        else
        {
            tag.Name = request.Name;
            tag.LastModified = _dateService.Now;

            await _dbContext.UpsertEntityAsync(tag, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(true, cancellationToken);

        return new UpsertTagCommandResponse
        {
            Value = tag.ToDto()
        };
    }
}
