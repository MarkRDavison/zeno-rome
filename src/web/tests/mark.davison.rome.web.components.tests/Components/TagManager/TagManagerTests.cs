namespace mark.davison.rome.web.components.tests.Components.TagManager;

public sealed class TagManagerTests
{
    [Test]
    public async Task RendersTagListTitleAndTagNames()
    {
        var tagId = Guid.NewGuid();
        var tagState = new Mock<ITagState>();
        tagState.Setup(_ => _.Tags).Returns([new TagDto(tagId, "Holiday")]);
        tagState.Setup(_ => _.Loading).Returns(false);
        tagState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);

        var client = new Mock<IClientHttpRepository>();
        var appContext = new Mock<IAppContextService>();

        using var ctx = new RomeComponentTestContext();
        ctx.Services.AddSingleton(tagState.Object);
        ctx.Services.AddSingleton(client.Object);
        ctx.Services.AddSingleton(appContext.Object);
        ctx.Services.AddSingleton(new TagManagerViewModel(appContext.Object, tagState.Object, client.Object));

        var cut = ctx.RenderComponent<TagManagerComponent>(parameters => parameters.Add(p => p.ShowClose, true));

        await Assert.That(cut.Find($"[data-testid='{DataTestIds.TagListTitle}']").TextContent).Contains("Tags");
        await Assert.That(cut.Find($"[data-testid='{DataTestIds.TagListItemPrefix}{tagId}']").TextContent).Contains("Holiday");
    }

    [Test]
    public async Task AddButton_DisabledWhenNameEmpty()
    {
        var tagState = new Mock<ITagState>();
        tagState.Setup(_ => _.Tags).Returns([]);
        tagState.Setup(_ => _.Loading).Returns(false);
        tagState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);

        var client = new Mock<IClientHttpRepository>();
        var appContext = new Mock<IAppContextService>();

        using var ctx = new RomeComponentTestContext();
        ctx.Services.AddSingleton(tagState.Object);
        ctx.Services.AddSingleton(client.Object);
        ctx.Services.AddSingleton(appContext.Object);
        ctx.Services.AddSingleton(new TagManagerViewModel(appContext.Object, tagState.Object, client.Object));

        var cut = ctx.RenderComponent<TagManagerComponent>();

        var addButton = cut.Find($"[data-testid='{DataTestIds.TagAddButton}']");
        await Assert.That(addButton.HasAttribute("disabled")).IsTrue();
    }
}
