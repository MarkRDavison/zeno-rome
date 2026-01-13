namespace mark.davison.rome.api.commands.tests.Scenarios.CreateTransaction.Validation;

public sealed class CreateTransctionValidationContextTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContext;
    private readonly CreateTransctionValidationContext _context;

    public CreateTransctionValidationContextTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));

        _currentUserContext = new(MockBehavior.Strict);
        _context = new CreateTransctionValidationContext(_dbContext);

        _currentUserContext.Setup(_ => _.UserId).Returns(Guid.Empty);

    }

    [Test]
    public async Task GetAccountById_FetchesFromRepository()
    {
        var accountId = Guid.NewGuid();

        await _dbContext.UpsertEntityAsync(new Account
        {
            Id = accountId,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var first = await _context.GetAccountById(accountId, CancellationToken.None);

        await Assert.That(first).IsNotNull();
    }
}
