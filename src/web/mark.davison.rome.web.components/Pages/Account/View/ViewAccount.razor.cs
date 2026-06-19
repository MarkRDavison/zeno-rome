namespace mark.davison.rome.web.components.Pages.Account.View;

public partial class ViewAccount : BaseView<ViewAccountViewModel, Guid>
{
    [Parameter]
    public required Guid Id { get; set; }

    protected override Task<bool> InitializeViewModel()
    {
        return ViewModel.Initialize(Id);
    }
}
