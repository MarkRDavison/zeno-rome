namespace mark.davison.rome.web.components.Pages.Transaction.View;

public class ViewTransactionViewModel : BaseViewModel<Guid>
{
    private Guid _transactionId;
    private readonly IAccountState _accountState;
    private readonly IStartupState _startupState;
    private readonly ITransactionState _transactionState;

    public ViewTransactionViewModel(
        IAccountState accountState,
        IStartupState startupState,
        ITransactionState transactionState)
    {
        _accountState = accountState;
        _startupState = startupState;
        _transactionState = transactionState;

        RegisterState(_accountState);
        RegisterState(_startupState);
        RegisterState(_transactionState);
    }

    public override async Task<bool> Initialize(Guid payload)
    {
        Console.WriteLine("#################### ViewTransactionViewModel - Initialize");
        _transactionId = payload;

        await _transactionState.FetchTransactions(_transactionId);
        Console.WriteLine("#################### ViewTransactionViewModel - _transactionId: {0}", _transactionId);
        Console.WriteLine("#################### ViewTransactionViewModel - _transactionState.Loading: {0}", _transactionState.Loading);

        return true;
    }

    public bool Loading
    {
        get
        {
            Console.WriteLine("#################### ViewTransactionViewModel - Loading get");
            return _transactionState.Loading || _transactionId == Guid.Empty;
        }
    }
}
