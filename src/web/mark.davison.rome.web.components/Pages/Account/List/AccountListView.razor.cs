namespace mark.davison.rome.web.components.Pages.Account.List;

public partial class AccountListView
{
    [Inject]
    public required AccountListViewModel ViewModel { get; set; }

    [Parameter]
    public Guid? Type { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if (await ViewModel.Initialize(Type))
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    public void AddAccount()
    {
        ViewModel.AddAccount();
    }

    public void Dispose()
    {
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }
}
