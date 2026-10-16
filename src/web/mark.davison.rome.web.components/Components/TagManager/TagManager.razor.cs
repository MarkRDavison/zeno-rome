namespace mark.davison.rome.web.components.Components.TagManager;

public partial class TagManager : ComponentBase, IDisposable
{
    private static readonly Dictionary<string, object> _tagAddNameInputAttributes = new()
    {
        ["data-testid"] = DataTestIds.TagAddName
    };

    private bool _disposed;

    [Inject]
    public required TagManagerViewModel ViewModel { get; set; }

    [Parameter]
    public bool ShowClose { get; set; }

    [Parameter]
    public EventCallback OnClose { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        await ViewModel.Initialize(null);
    }

    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    private async Task SaveNewAsync() => await ViewModel.SaveNewAsync();

    private async Task SaveEditAsync() => await ViewModel.SaveEditAsync();

    private async Task CloseAsync()
    {
        if (OnClose.HasDelegate)
        {
            await OnClose.InvokeAsync();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            _disposed = true;
        }
    }
}
