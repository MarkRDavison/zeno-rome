namespace mark.davison.rome.web.components.tests.Pages.Category.List;

public sealed class CategoryListViewModelTests
{
    [Test]
    public async Task CommandMenuItems_DoesNotIncludeDelete()
    {
        var viewModel = CreateViewModel();

        await Assert.That(viewModel.CommandMenuItems.Any(i => i.Id == "DELETE")).IsFalse();
        await Assert.That(viewModel.CommandMenuItems.Count).IsEqualTo(1);
        await Assert.That(viewModel.CommandMenuItems[0].Id).IsEqualTo("EDIT");
    }

    [Test]
    public void OnCommandMenuItemSelected_Edit_NavigatesToCategoryRoute()
    {
        var navigation = new Mock<IClientNavigationManager>();
        var viewModel = CreateViewModel(navigation);
        var categoryId = Guid.NewGuid();
        var item = new CategoryListItemViewModel { Id = categoryId };

        viewModel.OnCommandMenuItemSelected(item, new CommandMenuItem { Text = "Edit", Id = "EDIT" });

        navigation.Verify(_ => _.NavigateTo(RouteHelpers.Category(categoryId)), Times.Once);
    }

    private static CategoryListViewModel CreateViewModel(Mock<IClientNavigationManager>? navigation = null)
    {
        navigation ??= new Mock<IClientNavigationManager>();
        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.Categories).Returns([]);
        categoryState.Setup(_ => _.Loading).Returns(false);

        return new CategoryListViewModel(
            new Mock<IAppContextService>().Object,
            categoryState.Object,
            navigation.Object);
    }
}
