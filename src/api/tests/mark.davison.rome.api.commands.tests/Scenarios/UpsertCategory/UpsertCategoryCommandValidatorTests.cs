namespace mark.davison.rome.api.commands.tests.Scenarios.UpsertCategory;

public sealed class UpsertCategoryCommandValidatorTests
{
    private readonly Guid _userId;
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContext;
    private readonly UpsertCategoryCommandValidator _upsertCategoryCommandValidator;

    public UpsertCategoryCommandValidatorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContext = new(MockBehavior.Strict);
        _userId = Guid.NewGuid();
        _currentUserContext.Setup(_ => _.UserId).Returns(_userId);

        _upsertCategoryCommandValidator = new(_dbContext);
    }

    [Test]
    public async Task ValidateAsync_WhereNoCategoriesExist_ReturnsNoError()
    {
        var response = await _upsertCategoryCommandValidator.ValidateAsync(new(), _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    [Test]
    public async Task ValidateAsync_WhereNoConflictingCategoriesExist_ReturnsNoError()
    {
        var categories = new List<Category> {
            new Category
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Pets"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Car"
            }
        };
        await _dbContext.UpsertEntitiesAsync(categories, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertCategoryCommandRequest
        {
            Id = Guid.NewGuid(),
            Name = "Sports"
        };
        var response = await _upsertCategoryCommandValidator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    [Test]
    public async Task ValidateAsync_WhereConflictingCategoriesExist_ReturnsError()
    {
        var categories = new List<Category> {
            new Category
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Pets"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Car"
            }
        };
        await _dbContext.UpsertEntitiesAsync(categories, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertCategoryCommandRequest
        {
            Id = Guid.NewGuid(),
            Name = "Pets"
        };
        var response = await _upsertCategoryCommandValidator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(string.Format(UpsertCategoryCommandValidator.VALIDATION_DUPLICATE_CATEGORY, request.Name));
    }

    [Test]
    public async Task ValidateAsync_WhereConflictingCategoriesExistButForSameId_ReturnsNoError()
    {
        var categories = new List<Category> {
            new Category
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Pets"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Car"
            }
        };
        await _dbContext.UpsertEntitiesAsync(categories, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertCategoryCommandRequest
        {
            Id = categories.First().Id,
            Name = categories.First().Name
        };
        var response = await _upsertCategoryCommandValidator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }
}
