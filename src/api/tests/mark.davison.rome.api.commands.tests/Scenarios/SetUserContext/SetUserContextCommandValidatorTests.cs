namespace mark.davison.rome.api.commands.tests.Scenarios.SetUserContext;

public sealed class SetUserContextCommandValidatorTests
{
    private readonly SetUserContextCommandValidator _validator = new();
    private readonly Mock<ICurrentUserContext> _currentUserContextMock = new(MockBehavior.Strict);

    [Test]
    public async Task ValidateAsync_WhenStartAfterEnd_ReturnsError()
    {
        var request = new SetUserContextCommandRequest
        {
            StartRange = new DateOnly(2026, 3, 31),
            EndRange = new DateOnly(2026, 3, 1)
        };

        var response = await _validator.ValidateAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsFalse();
        await Assert.That(response.Errors).Contains(SetUserContextCommandValidator.VALIDATION_INVALID_DATE_RANGE);
    }

    [Test]
    public async Task ValidateAsync_WhenRangeValid_ReturnsSuccess()
    {
        var request = new SetUserContextCommandRequest
        {
            StartRange = new DateOnly(2026, 3, 1),
            EndRange = new DateOnly(2026, 3, 31)
        };

        var response = await _validator.ValidateAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.Errors).IsEmpty();
    }
}
