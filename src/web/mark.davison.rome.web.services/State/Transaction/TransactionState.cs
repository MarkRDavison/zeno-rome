using mark.davison.rome.shared.models.dto.Scenarios.Queries.TransactionByAccount;

namespace mark.davison.rome.web.services.State.Transaction;

internal sealed class TransactionState : ITransactionState
{
    private readonly IClientHttpRepository _clientRepository;

    public TransactionState(IClientHttpRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public event EventHandler StateChanged = default!;
    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }
    public IList<TransactionDto> Transactions { get; private set; } = [];

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetState(IList<TransactionDto> transactions)
    {
        Transactions = [.. transactions];
        Loading = false;
        Loaded = true;
        NotifyStateChanged();
    }

    public async Task FetchTransactions(Guid transactionGroupId)
    {
        Transactions = [];
        Loading = true;
        Loaded = false;

        NotifyStateChanged();

        var request = new FetchTransactionQueryRequest { TransactionGroupId = transactionGroupId };
        var response = await _clientRepository.Get<FetchTransactionQueryRequest, FetchTransactionQueryResponse>(request, CancellationToken.None);

        if (response.SuccessWithValue)
        {
            SetState([.. response.Value]); // TODO: Dont just replace...
        }
        else
        {
            SetState([]);
        }
    }

    public async Task FetchTransactionsForAccount(Guid accountId)
    {
        Transactions = [];
        Loading = true;
        Loaded = false;

        NotifyStateChanged();
        var request = new TransactionByAccountQueryRequest { AccountId = accountId };
        var response = await _clientRepository.Get<TransactionByAccountQueryRequest, TransactionByAccountQueryResponse>(request, CancellationToken.None);

        if (response.SuccessWithValue)
        {
            SetState([.. response.Value]); // TODO: Dont just replace...
        }
        else
        {
            SetState([]);
        }
    }
}
