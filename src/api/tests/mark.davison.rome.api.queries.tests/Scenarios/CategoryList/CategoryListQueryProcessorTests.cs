namespace mark.davison.rome.api.queries.tests.Scenarios.CategoryList;

public class CategoryListQueryProcessorTests
{
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly CategoryListQueryProcessor _categoryListQueryProcessor;
    private readonly Guid _userId;

    public CategoryListQueryProcessorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContextMock = new(MockBehavior.Strict);
        _userId = Guid.NewGuid();
        _currentUserContextMock.Setup(_ => _.UserId).Returns(_userId);

        _categoryListQueryProcessor = new(_dbContext);
    }

    [Test]
    public async Task ProcessAsync_ReturnsCategories()
    {
        var category1 = new Category
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = _userId,
            Name = Guid.NewGuid().ToString()
        };

        var category2 = new Category
        {
            Id = Guid.NewGuid(),
            Created = DateTime.Now,
            LastModified = DateTime.Now,
            UserId = _userId,
            Name = Guid.NewGuid().ToString()
        };

        await _dbContext.UpsertEntitiesAsync([category1, category2], CancellationToken.None);
        await _dbContext.SaveChangesAsync(true, CancellationToken.None);

        var request = new CategoryListQueryRequest();
        var response = await _categoryListQueryProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();
        await Assert.That(response.Value).Count().IsEqualTo(2);
        await Assert.That(response.Value).Contains(_ => _.Name == category1.Name);
        await Assert.That(response.Value).Contains(_ => _.Name == category2.Name);
    }
}