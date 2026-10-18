using mark.davison.common.client.web.abstractions.Authentication;
using mark.davison.rome.shared.models.dto.Scenarios.Commands.SetUserContext;

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

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    [AllowNull]
    private MudDateRangePicker _picker;

    private DateRange _range = new();
    private bool _changeInProgress;

    protected override void OnInitialized()
    {
        AuthenticationService.UserChanged += AuthenticationService_UserChanged;
    }

    private void AuthenticationService_UserChanged(object? sender, ClaimsPrincipal e)
    {
        _ = HandleAuthStateChanged(e);
    }

    private async Task HandleAuthStateChanged(ClaimsPrincipal authState)
    {
        UpdateRangeFromAppContextService();
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnParametersSet()
    {
        UpdateRangeFromAppContextService();
    }

    private void UpdateRangeFromAppContextService()
    {
        var start = AppContextService.State.RangeStart.ToDateTime(TimeOnly.MinValue);
        var end = AppContextService.State.RangeEnd.ToDateTime(TimeOnly.MinValue);

        _range = new(start, end);
    }

    public void Dispose()
    {
        AuthenticationService.UserChanged -= AuthenticationService_UserChanged;
    }

    private async Task OpenPicker()
    {
        await _picker.OpenAsync();
    }

    private async Task Close()
    {
        await _picker.CloseAsync();
    }

    private async Task Reset()
    {
        await _picker.CloseAsync();
        var (s, e) = DateRules.GetMonthRange(DateOnly.FromDateTime(DateTime.Today));
        await DateRangeChanged(new DateRange(s.ToDateTime(TimeOnly.MinValue), e.ToDateTime(TimeOnly.MinValue)));
    }

    private async Task DateRangeChanged(DateRange range)
    {
        if (_changeInProgress || range.Start is null || range.End is null)
        {
            return;
        }

        var start = DateOnly.FromDateTime(range.Start.Value);
        var end = DateOnly.FromDateTime(range.End.Value);

        if (start > end)
        {
            Snackbar.Add("End date must be on or after start date.", Severity.Error);
            UpdateRangeFromAppContextService();
            await InvokeAsync(StateHasChanged);
            return;
        }

        _changeInProgress = true;

        _range = range;

        try
        {
            var request = new SetUserContextCommandRequest
            {
                StartRange = start,
                EndRange = end
            };

            var response = await ClientHttpRepository.Post<SetUserContextCommandRequest, SetUserContextCommandResponse>(request, CancellationToken.None);

            if (response.SuccessWithValue)
            {
                _range = new(
                    response.Value.StartRange.ToDateTime(TimeOnly.MinValue),
                    response.Value.EndRange.ToDateTime(TimeOnly.MinValue));

                AppContextService.UpdateRange(
                    DateOnly.FromDateTime(_range.Start!.Value),
                    DateOnly.FromDateTime(_range.End!.Value));
            }
            else
            {
                Snackbar.Add("Could not save date range. Please try again.", Severity.Error);
                UpdateRangeFromAppContextService();
                await InvokeAsync(StateHasChanged);
            }
        }
        finally
        {
            _changeInProgress = false;
        }
    }

    private string FormattedRange => $"{DateOnly.FromDateTime(_range.Start!.Value).ToOrdinalShortDate()} - {DateOnly.FromDateTime(_range.End!.Value).ToOrdinalShortDate()}";

    private string Username => ClaimsPrincipal?.Claims?.FirstOrDefault(_ => _?.Type == ClaimTypes.Name)?.Value ?? string.Empty;
}
