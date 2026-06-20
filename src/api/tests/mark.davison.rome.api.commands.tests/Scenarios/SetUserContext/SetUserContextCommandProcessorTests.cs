namespace mark.davison.rome.api.commands.tests.Scenarios.SetUserContext;

public class SetUserContextCommandProcessorTests
{
    private readonly Mock<IFinanceUserContext> _financeUserContextMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly SetUserContextCommandProcessor _setUserContextCommandProcessor;

    public SetUserContextCommandProcessorTests()
    {
        _financeUserContextMock = new(MockBehavior.Strict);
        _currentUserContextMock = new(MockBehavior.Strict);

        _setUserContextCommandProcessor = new(_financeUserContextMock.Object);
    }

    [Test]
    public async Task ProcessAsync_SetsContextOn_FinanceUserContext()
    {
        var request = new SetUserContextCommandRequest
        {
            StartRange = DateOnly.FromDateTime(DateTime.Today).AddDays(-20),
            EndRange = DateOnly.FromDateTime(DateTime.Today).AddDays(20)
        };

        _financeUserContextMock
            .Setup(_ => _.SetAsync(request.StartRange, request.EndRange, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var response = await _setUserContextCommandProcessor.ProcessAsync(request, _currentUserContextMock.Object, CancellationToken.None);

        await Assert.That(response.SuccessWithValue).IsTrue();

        await Assert.That(response.Value!.StartRange).IsEqualTo(request.StartRange);
        await Assert.That(response.Value!.EndRange).IsEqualTo(request.EndRange);

        _financeUserContextMock
            .Verify(
                _ => _.SetAsync(request.StartRange, request.EndRange, It.IsAny<CancellationToken>()),
                Times.Once);
    }
}
