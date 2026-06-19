namespace mark.davison.rome.web.components.Forms.EditTransaction;

public class EditTransactionFormSubmission : IFormSubmission<EditTransactionFormViewModel>
{
    private readonly IAccountState _accountState;
    private readonly IClientHttpRepository _clientHttpRepository;

    public EditTransactionFormSubmission(
        IAccountState accountState,
        IClientHttpRepository clientHttpRepository)
    {
        _accountState = accountState;
        _clientHttpRepository = clientHttpRepository;
    }

    public async Task<Response> Primary(EditTransactionFormViewModel formViewModel)
    {
        var request = new CreateTransactionCommandRequest
        {
            Description = formViewModel.SplitDescription,
            TransactionTypeId = formViewModel.TransactionTypeId,
            Transactions = [.. formViewModel.Items.Select(_ => ToCreateTransactionDto(_, formViewModel))]
        };

        var response = await _clientHttpRepository
            .Post<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(
                request,
                CancellationToken.None);

        if (response.Success)
        {
            // TODO: Trigger current balance flags dirty for accounts in transaction
            if (formViewModel.Id == Guid.Empty)
            {
                formViewModel.Id = response.Group.Id;
            }
        }

        return response;
    }

    private CreateTransactionDto ToCreateTransactionDto(EditTransactionFormViewModelItem item, EditTransactionFormViewModel formViewModel)
    {
        var account = _accountState.Accounts.FirstOrDefault(_ => _.Id == item.SourceAccountId);

        return new CreateTransactionDto(
            item.Id,
            item.Description,
            item.SourceAccountId!.Value,
            item.DestinationAccountId!.Value,
            DateOnly.FromDateTime(formViewModel.Date!.Value),
            CurrencyRules.ToPersisted(item.Amount!.Value),
            item.ForeignAmount == null ? null : CurrencyRules.ToPersisted(item.ForeignAmount!.Value),
            account?.CurrencyId ?? Guid.Empty,
            item.ForeignCurrencyId); // TODO: CATEGORY
    }
}
