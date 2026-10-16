namespace mark.davison.rome.web.components.tests.Components.TagManager;

public sealed class TagManagerViewModelTests
{
    [Test]
    public async Task SaveNewAsync_Success_CallsUpsertAndFetchState()
    {
        var tagState = new Mock<ITagState>();
        tagState.Setup(_ => _.Tags).Returns([]);
        tagState.Setup(_ => _.Loading).Returns(false);
        tagState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);

        var client = new Mock<IClientHttpRepository>();
        client
            .Setup(_ => _.Post<UpsertTagCommandRequest, UpsertTagCommandResponse>(It.IsAny<UpsertTagCommandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpsertTagCommandResponse { Value = new TagDto(Guid.NewGuid(), "Holiday") });

        var appContext = new Mock<IAppContextService>();
        var viewModel = new TagManagerViewModel(appContext.Object, tagState.Object, client.Object)
        {
            NewTagName = "Holiday"
        };

        await viewModel.SaveNewAsync();

        client.Verify(_ => _.Post<UpsertTagCommandRequest, UpsertTagCommandResponse>(It.IsAny<UpsertTagCommandRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        tagState.Verify(_ => _.FetchState(), Times.Once);
    }

    [Test]
    public async Task SaveNewAsync_Duplicate_ShowsError()
    {
        var tagState = new Mock<ITagState>();
        tagState.Setup(_ => _.Tags).Returns([]);
        tagState.Setup(_ => _.Loading).Returns(false);

        var client = new Mock<IClientHttpRepository>();
        client
            .Setup(_ => _.Post<UpsertTagCommandRequest, UpsertTagCommandResponse>(It.IsAny<UpsertTagCommandRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpsertTagCommandResponse { Errors = ["VALIDATION_DUPLICATE_TAGHoliday"] });

        var appContext = new Mock<IAppContextService>();
        var viewModel = new TagManagerViewModel(appContext.Object, tagState.Object, client.Object)
        {
            NewTagName = "Holiday"
        };

        await viewModel.SaveNewAsync();

        await Assert.That(viewModel.ErrorMessage).IsNotNull();
        await Assert.That(viewModel.ErrorMessage).Contains("VALIDATION_DUPLICATE_TAG");
    }
}
