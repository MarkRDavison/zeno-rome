namespace mark.davison.rome.web.components.CommonCandidates;

public abstract class BaseView<TViewModel, TPayload> : ComponentBase, IDisposable
    where TViewModel : BaseViewModel<TPayload>
{
    private bool _disposedValue;

    [Inject]
    public required TViewModel ViewModel { get; set; }

    protected override async Task OnInitializedAsync()
    {
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        await base.SetParametersAsync(parameters);

        if (await InitializeViewModel())
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    protected abstract Task<bool> InitializeViewModel();

    private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        await InvokeAsync(StateHasChanged);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
