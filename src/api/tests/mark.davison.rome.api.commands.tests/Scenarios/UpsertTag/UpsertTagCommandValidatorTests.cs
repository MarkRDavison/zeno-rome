namespace mark.davison.rome.api.commands.tests.Scenarios.UpsertTag;

public sealed class UpsertTagCommandValidatorTests
{
    private readonly Guid _userId;
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly Mock<ICurrentUserContext> _currentUserContext;
    private readonly UpsertTagCommandValidator _upsertTagCommandValidator;

    public UpsertTagCommandValidatorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContext = new(MockBehavior.Strict);
        _userId = Guid.NewGuid();
        _currentUserContext.Setup(_ => _.UserId).Returns(_userId);

        _upsertTagCommandValidator = new(_dbContext);
    }

    [Test]
    public async Task ValidateAsync_WhereNoTagsExist_ReturnsNoError()
    {
        var response = await _upsertTagCommandValidator.ValidateAsync(new(), _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    [Test]
    public async Task ValidateAsync_WhereNoConflictingTagsExist_ReturnsNoError()
    {
        var tags = new List<Tag> {
            new Tag
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Holiday"
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Work"
            }
        };
        await _dbContext.UpsertEntitiesAsync(tags, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertTagCommandRequest
        {
            Id = Guid.NewGuid(),
            Name = "Personal"
        };
        var response = await _upsertTagCommandValidator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    [Test]
    public async Task ValidateAsync_WhereConflictingTagsExist_ReturnsError()
    {
        var tags = new List<Tag> {
            new Tag
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Holiday"
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Work"
            }
        };
        await _dbContext.UpsertEntitiesAsync(tags, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertTagCommandRequest
        {
            Id = Guid.NewGuid(),
            Name = "Holiday"
        };
        var response = await _upsertTagCommandValidator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(string.Format(UpsertTagCommandValidator.VALIDATION_DUPLICATE_TAG, request.Name));
    }

    [Test]
    public async Task ValidateAsync_WhereConflictingTagsExistButForSameId_ReturnsNoError()
    {
        var tags = new List<Tag> {
            new Tag
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Holiday"
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                LastModified = DateTime.Now,
                UserId = _userId,
                Name = "Work"
            }
        };
        await _dbContext.UpsertEntitiesAsync(tags, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertTagCommandRequest
        {
            Id = tags.First().Id,
            Name = tags.First().Name
        };
        var response = await _upsertTagCommandValidator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }
}
