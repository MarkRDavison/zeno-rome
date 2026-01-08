using mark.davison.common.client.web.abstractions.Authentication;

namespace mark.davison.rome.web.components.Controls;

public partial class AppContext
{
    [Inject]
    public required IAppContextService AppContextService { get; set; }

    [Parameter, EditorRequired]
    public required ClaimsPrincipal ClaimsPrincipal { get; set; }

    [Inject]
    public required IClientHttpRepository ClientHttpRepository { get; set; }

    [Inject]
    public required IAuthenticationService AuthenticationService { get; set; }

    [AllowNull]
    private MudDateRangePicker _picker;

    private readonly DateRange _range = new();
    private bool _changeInProgress;

    protected override void OnInitialized()
    {
        AuthenticationService.UserChanged += AuthenticationService_UserChanged;
    }

    private void AuthenticationService_UserChanged(object? sender, ClaimsPrincipal e)
    {
        Console.WriteLine("AppContext.razor.cs: AuthenticationService_UserChanged");
        _ = HandleAuthStateChanged(e);
    }

    private async Task HandleAuthStateChanged(ClaimsPrincipal authState)
    {
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        UpdateRangeFromAppContextService();
    }

    private void UpdateRangeFromAppContextService()
    {
        _range.Start = AppContextService.State.RangeStart.ToDateTime(TimeOnly.MinValue);
        _range.End = AppContextService.State.RangeEnd.ToDateTime(TimeOnly.MinValue);
    }

    public void Dispose()
    {
        AuthenticationService.UserChanged -= AuthenticationService_UserChanged;
    }

    private async Task OpenPicker()
    {
        await _picker.OpenAsync();
    }

    private async Task Reset()
    {
        await _picker.CloseAsync();
        var (s, e) = DateRules.GetMonthRange(DateOnly.FromDateTime(DateTime.Today));
        await DateRangeChanged(new DateRange(s.ToDateTime(TimeOnly.MinValue), e.ToDateTime(TimeOnly.MinValue)));
    }

    private async Task DateRangeChanged(DateRange range)
    {
        if (!_changeInProgress)
        {
            _changeInProgress = true;

            try
            {
                //var request = new SetUserContextCommandRequest
                //{
                //    UserContext = new UserContextDto
                //    {
                //        StartRange = DateOnly.FromDateTime(range.Start!.Value),
                //        EndRange = DateOnly.FromDateTime(range.End!.Value)
                //    }
                //};

                //var response = await ClientHttpRepository.Post<SetUserContextCommandResponse, SetUserContextCommandRequest>(request, CancellationToken.None);

                //if (response.SuccessWithValue)
                //{
                //    _range.Start = response.Value.StartRange.ToDateTime(TimeOnly.MinValue);
                //    _range.End = response.Value.EndRange.ToDateTime(TimeOnly.MinValue);

                //    AppContextService.UpdateRange(
                //        DateOnly.FromDateTime(range.Start!.Value),
                //        DateOnly.FromDateTime(range.End!.Value));
                //}

                AppContextService.UpdateRange(
                    DateOnly.FromDateTime(range.Start!.Value),
                    DateOnly.FromDateTime(range.End!.Value));
            }
            finally
            {
                _changeInProgress = false;
            }

            UpdateRangeFromAppContextService();

            await InvokeAsync(StateHasChanged);
        }
    }

    private string FormattedRange => $"{DateOnly.FromDateTime(_range.Start!.Value).ToOrdinalShortDate()} - {DateOnly.FromDateTime(_range.End!.Value).ToOrdinalShortDate()}";

    private string Username => ClaimsPrincipal?.Claims?.FirstOrDefault(_ => _?.Type == ClaimTypes.Name)?.Value ?? string.Empty;
}
