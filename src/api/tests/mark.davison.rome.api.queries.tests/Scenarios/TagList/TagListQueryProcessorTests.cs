namespace mark.davison.rome.api.queries.tests.Scenarios.TagList;

public class TagListQueryProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly TagListQueryProcessor _tagListQueryProcessor;
    private readonly Guid _userId;

    public TagListQueryProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);
        _userId = Guid.NewGuid();
        _currentUserContextMock.Setup(_ => _.UserId).Returns(_userId);

        _tagListQueryProcessor = new(_dbContext);
    }

    [Test]
    public async Task ProcessAsync_ReturnsTags()
    {
        var tag1 = new Tag
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = _userId,
            Name = Guid.NewGuid().ToString()
        };

        var tag2 = new Tag
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = _userId,
            Name = Guid.NewGuid().ToString()
        };

        await _dbContext.UpsertEntitiesAsync([tag1, tag2], CancellationToken.None);
        await _dbContext.SaveChangesAsync(true, CancellationToken.None);

        var request = new TagListQueryRequest();
        var response = await _tagListQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(2);
        await Assert.That(response.Value).Contains(_ => _.Name == tag1.Name);
        await Assert.That(response.Value).Contains(_ => _.Name == tag2.Name);
    }

    [Test]
    public async Task ProcessAsync_WhenUserHasNoTags_ReturnsEmptyList()
    {
        var request = new TagListQueryRequest();
        var response = await _tagListQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).IsEmpty();
    }

    [Test]
    public async Task ProcessAsync_DoesNotReturnOtherUsersTags()
    {
        var otherUserTag = new Tag
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = Guid.NewGuid(),
            Name = "Other user tag"
        };

        var userTag = new Tag
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = _userId,
            Name = "My tag"
        };

        await _dbContext.UpsertEntitiesAsync([otherUserTag, userTag], CancellationToken.None);
        await _dbContext.SaveChangesAsync(true, CancellationToken.None);

        var request = new TagListQueryRequest();
        var response = await _tagListQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(1);
        await Assert.That(response.Value!.Single().Name).IsEqualTo(userTag.Name);
    }
}
