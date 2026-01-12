namespace mark.davison.rome.web.components.Forms.EditAccount;

public class EditAccountFormSubmission : IFormSubmission<EditAccountFormViewModel>
{
    private readonly IStartupState _startupState;
    private readonly IClientHttpRepository _clientHttpRepository;

    public EditAccountFormSubmission(IStartupState startupState, IClientHttpRepository clientHttpRepository)
    {
        _startupState = startupState;
        _clientHttpRepository = clientHttpRepository;
    }

    public async Task<Response> Primary(EditAccountFormViewModel formViewModel)
    {
        if (formViewModel.Id == Guid.Empty)
        {
            formViewModel.Id = Guid.NewGuid();
        }

        var currency = _startupState.Currencies.First(_ => _.Id == formViewModel.CurrencyId);
        var accountType = _startupState.AccountTypes.First(_ => _.Id == formViewModel.AccountTypeId);

        bool openingBalanceSpecified = formViewModel.OpeningBalance != default;

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(
                formViewModel.Id,
                formViewModel.Name,
                CurrencyRules.ToPersisted(formViewModel.VirtualBalance ?? 0),
                formViewModel.AccountNumber,
                formViewModel.AccountTypeId ?? Guid.Empty,
                formViewModel.CurrencyId ?? Guid.Empty,
                openingBalanceSpecified ? CurrencyRules.ToPersisted(formViewModel.OpeningBalance ?? 0) : null,
                openingBalanceSpecified && formViewModel.OpeningBalanceDate != null ? DateOnly.FromDateTime(formViewModel.OpeningBalanceDate.Value) : null)
        };

        var response = await _clientHttpRepository.Post<UpsertAccountCommandRequest, UpsertAccountCommandResponse>(request, CancellationToken.None);

        return response;
    }
}
