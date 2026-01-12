using mark.davison.common.client.abstractions.Repository;
using mark.davison.rome.shared.models.dto.Scenarios.Queries.AccountList;

namespace mark.davison.rome.web.services.State;

internal sealed class AccountState : IAccountState
{
    private readonly IClientHttpRepository _clientRepository;

    public AccountState(IClientHttpRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public bool Loading { get; private set; }
    public bool Loaded { get; private set; }

    public event EventHandler StateChanged = default!;
    public IList<AccountDto> Accounts { get; private set; } = [];

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetState(IList<AccountDto> accounts)
    {
        Accounts = [.. accounts];
        Loading = false;
        Loaded = true;
        NotifyStateChanged();
    }

    public async Task FetchState(Guid? accountTypeId)
    {
        Accounts = [];
        Loading = true;
        Loaded = false;

        NotifyStateChanged();

        var request = new AccountListQueryRequest { AccountType = accountTypeId };
        var response = await _clientRepository.Get<AccountListQueryRequest, AccountListQueryResponse>(request, CancellationToken.None);

        if (response.SuccessWithValue)
        {
            SetState([.. response.Value]);
        }
        else
        {
            SetState([]);
        }
    }
}
