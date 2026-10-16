namespace mark.davison.rome.web.components.tests.Pages.Category.Edit;

public sealed class EditCategoryViewModelTests
{
    [Test]
    public async Task Initialize_WithKnownId_LoadsFormFromCategoryState()
    {
        var categoryId = Guid.NewGuid();
        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);
        categoryState.Setup(_ => _.Categories).Returns(new List<CategoryDto> { new(categoryId, "Groceries") });
        categoryState.Setup(_ => _.Loading).Returns(false);
        categoryState.Setup(_ => _.Loaded).Returns(true);

        var viewModel = CreateViewModel(categoryState);

        await viewModel.Initialize(categoryId);

        await Assert.That(viewModel.FormViewModel).IsNotNull();
        await Assert.That(viewModel.FormViewModel!.Id).IsEqualTo(categoryId);
        await Assert.That(viewModel.FormViewModel.Name).IsEqualTo("Groceries");
        await Assert.That(viewModel.Title).IsEqualTo("Edit category");
    }

    [Test]
    public async Task Initialize_WithUnknownId_NavigatesToNotFound()
    {
        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);
        categoryState.Setup(_ => _.Categories).Returns([]);

        var navigation = new Mock<IClientNavigationManager>();
        var viewModel = CreateViewModel(categoryState, navigation: navigation);

        await viewModel.Initialize(Guid.NewGuid());

        navigation.Verify(_ => _.NavigateTo(Routes.NotFound), Times.Once);
    }

    [Test]
    public async Task OnSave_EditSuccess_RefreshesStateAndNavigatesToList()
    {
        var categoryId = Guid.NewGuid();
        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);
        categoryState.Setup(_ => _.Categories).Returns(new List<CategoryDto> { new(categoryId, "Groceries") });
        categoryState.Setup(_ => _.Loading).Returns(false);
        categoryState.Setup(_ => _.Loaded).Returns(true);

        var submission = new Mock<IFormSubmission<EditCategoryFormViewModel>>();
        submission
            .Setup(_ => _.Primary(It.IsAny<EditCategoryFormViewModel>()))
            .ReturnsAsync(new UpsertCategoryCommandResponse { Value = new CategoryDto(categoryId, "Groceries updated") });

        var navigation = new Mock<IClientNavigationManager>();
        var viewModel = CreateViewModel(categoryState, submission, navigation);

        await viewModel.Initialize(categoryId);
        viewModel.FormViewModel!.Name = "Groceries updated";

        await viewModel.OnSave();

        categoryState.Verify(_ => _.FetchState(), Times.AtLeastOnce);
        navigation.Verify(_ => _.NavigateTo(Routes.Categories), Times.Once);
    }

    private static EditCategoryViewModel CreateViewModel(
        Mock<ICategoryState>? categoryState = null,
        Mock<IFormSubmission<EditCategoryFormViewModel>>? submission = null,
        Mock<IClientNavigationManager>? navigation = null)
    {
        if (categoryState is null)
        {
            categoryState = new Mock<ICategoryState>();
            categoryState.Setup(_ => _.Categories).Returns([]);
            categoryState.Setup(_ => _.Loading).Returns(false);
            categoryState.Setup(_ => _.Loaded).Returns(true);
            categoryState.Setup(_ => _.FetchState()).Returns(Task.CompletedTask);
        }

        submission ??= new Mock<IFormSubmission<EditCategoryFormViewModel>>();
        navigation ??= new Mock<IClientNavigationManager>();

        return new EditCategoryViewModel(
            new Mock<IAppContextService>().Object,
            categoryState.Object,
            navigation.Object,
            submission.Object);
    }
}
