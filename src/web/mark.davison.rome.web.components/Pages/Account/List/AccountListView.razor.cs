namespace mark.davison.rome.web.components.Pages.Account.List;

public partial class AccountListView : BaseView<AccountListViewModel, Guid?>
{
    [Parameter]
    public Guid? Type { get; set; }

    protected override async Task<bool> InitializeViewModel()
    {
        return await ViewModel.Initialize(Type);
    }
}
