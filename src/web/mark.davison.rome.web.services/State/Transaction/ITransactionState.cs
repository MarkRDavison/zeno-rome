namespace mark.davison.rome.web.services.State.Transaction;

public interface ITransactionState : IStateService
{
    IList<TransactionDto> Transactions { get; }
    void SetState(IList<TransactionDto> transactions);
    Task FetchTransactions(Guid transactionGroupId);
    Task FetchTransactionsForAccount(Guid accountId);
}
