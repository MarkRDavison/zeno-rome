namespace mark.davison.rome.api.commands.tests.Scenarios.UpsertCategory;

public class UpsertCategoryCommandProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IDateService> _dateService;
    private readonly UpsertCategoryCommandProcessor _upsertCategoryCommandProcessor;
    private readonly Guid _userId;

    public UpsertCategoryCommandProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);
        _dateService = new(MockBehavior.Strict);
        _userId = Guid.NewGuid();

        _currentUserContextMock.Setup(_ => _.UserId).Returns(_userId);
        _dateService.Setup(_ => _.Now).Returns(DateTime.Now);

        _upsertCategoryCommandProcessor = new(_dbContext, _dateService.Object);
    }

    [Test]
    public async Task ProcessAsync_ForExistingCategory_UpdatesLastModifiedAndName()
    {
        var existingCategory = new Category
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = _userId,
            Name = "Pets"
        };

        await _dbContext.UpsertEntitiesAsync([existingCategory], CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertCategoryCommandRequest
        {
            Id = existingCategory.Id,
            Name = "Pet"
        };

        var response = await _upsertCategoryCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value!.Id).IsEqualTo(existingCategory.Id);
        await Assert.That(response.Value.Name).IsEqualTo(request.Name);

        var persisted = await _dbContext.GetByIdAsync<Category>(request.Id, CancellationToken.None);

        await Assert.That(persisted).IsNotNull();
        await Assert.That(persisted.Id).IsEqualTo(request.Id);
        await Assert.That(persisted.Name).IsEqualTo(request.Name);
    }

    [Test]
    public async Task ProcessAsync_ForNewCategory_PersistsNewCategory()
    {
        var request = new UpsertCategoryCommandRequest
        {
            Id = Guid.NewGuid(),
            Name = "Pet"
        };

        var response = await _upsertCategoryCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value!.Id).IsEqualTo(request.Id);
        await Assert.That(response.Value.Name).IsEqualTo(request.Name);

        var persisted = await _dbContext.GetByIdAsync<Category>(request.Id, CancellationToken.None);

        await Assert.That(persisted).IsNotNull();
        await Assert.That(persisted.Id).IsEqualTo(request.Id);
        await Assert.That(persisted.Name).IsEqualTo(request.Name);
    }
}
