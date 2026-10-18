namespace mark.davison.rome.api.commands.tests.Scenarios.UpsertUserPreferences;

public sealed class UpsertUserPreferencesCommandValidatorTests
{
    private readonly UpsertUserPreferencesCommandValidator _validator = new();
    private readonly Mock<ICurrentUserContext> _currentUserContextMock = new(MockBehavior.Strict);

    [Test]
    public async Task ValidateAsync_WhenCountOutOfRange_ReturnsError()
    {
        var request = new UpsertUserPreferencesCommandRequest
        {
            Preferences = new UserPreferencesDto { HomeRecentTransactionCount = 2 }
        };

        var response = await _validator.ValidateAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.Errors).IsNotEmpty();
    }

    [Test]
    public async Task ValidateAsync_WhenCountInRange_Succeeds()
    {
        var request = new UpsertUserPreferencesCommandRequest
        {
            Preferences = new UserPreferencesDto { HomeRecentTransactionCount = 25 }
        };

        var response = await _validator.ValidateAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.Errors).IsEmpty();
    }
}
