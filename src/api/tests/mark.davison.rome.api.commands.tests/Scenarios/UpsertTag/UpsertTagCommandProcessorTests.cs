namespace mark.davison.rome.api.commands.tests.Scenarios.UpsertTag;

public class UpsertTagCommandProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IDateService> _dateService;
    private readonly UpsertTagCommandProcessor _upsertTagCommandProcessor;
    private readonly Guid _userId;

    public UpsertTagCommandProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);
        _dateService = new(MockBehavior.Strict);
        _userId = Guid.NewGuid();

        _currentUserContextMock.Setup(_ => _.UserId).Returns(_userId);
        _dateService.Setup(_ => _.Now).Returns(DateTime.Now);

        _upsertTagCommandProcessor = new(_dbContext, _dateService.Object);
    }

    [Test]
    public async Task ProcessAsync_ForExistingTag_UpdatesLastModifiedAndName()
    {
        var existingTag = new Tag
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = _userId,
            Name = "Holiday"
        };

        await _dbContext.UpsertEntitiesAsync([existingTag], CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertTagCommandRequest
        {
            Id = existingTag.Id,
            Name = "Vacation"
        };

        var response = await _upsertTagCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value!.Id).IsEqualTo(existingTag.Id);
        await Assert.That(response.Value.Name).IsEqualTo(request.Name);

        var persisted = await _dbContext.GetByIdAsync<Tag>(request.Id, CancellationToken.None);

        await Assert.That(persisted).IsNotNull();
        await Assert.That(persisted.Id).IsEqualTo(request.Id);
        await Assert.That(persisted.Name).IsEqualTo(request.Name);
    }

    [Test]
    public async Task ProcessAsync_ForNewTag_PersistsNewTag()
    {
        var request = new UpsertTagCommandRequest
        {
            Id = Guid.NewGuid(),
            Name = "Vacation"
        };

        var response = await _upsertTagCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value!.Id).IsEqualTo(request.Id);
        await Assert.That(response.Value.Name).IsEqualTo(request.Name);

        var persisted = await _dbContext.GetByIdAsync<Tag>(request.Id, CancellationToken.None);

        await Assert.That(persisted).IsNotNull();
        await Assert.That(persisted.Id).IsEqualTo(request.Id);
        await Assert.That(persisted.Name).IsEqualTo(request.Name);
    }
}
