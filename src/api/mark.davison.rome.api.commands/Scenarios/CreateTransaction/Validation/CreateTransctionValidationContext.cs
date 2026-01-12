namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

internal sealed class CreateTransctionValidationContext : ICreateTransctionValidationContext
{
    private readonly IDbContext<RomeDbContext> _dbContext;

    private readonly IDictionary<Guid, Account?> _accounts;
    //private readonly IDictionary<Guid, Category?> _categories;

    public CreateTransctionValidationContext(
        IDbContext<RomeDbContext> dbContext
    )
    {
        _dbContext = dbContext;

        _accounts = new Dictionary<Guid, Account?>();
        //_categories = new Dictionary<Guid, Category?>();
    }

    public Task<Account?> GetAccountById(Guid accountId, CancellationToken cancellationToken) => GetEntityById(accountId, _accounts, cancellationToken);

    //public Task<Category?> GetCategoryById(Guid categoryId, CancellationToken cancellationToken) => GetEntityById(categoryId, _categories, cancellationToken);

    private async Task<T?> GetEntityById<T>(Guid id, IDictionary<Guid, T?> cache, CancellationToken cancellationToken)
        where T : BaseEntity
    {
        if (cache.TryGetValue(id, out var entity))
        {
            return entity;
        }

        var newEntity = await _dbContext.GetByIdAsync<T>(id, cancellationToken);

        cache.Add(id, newEntity);

        return newEntity;
    }
}