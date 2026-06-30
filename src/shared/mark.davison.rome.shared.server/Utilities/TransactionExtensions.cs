namespace mark.davison.rome.shared.server.Utilities;

public static class TransactionExtensions
{
    public static TransactionDto ToDto(this Transaction transaction, TransactionJournal transactionJournal, TransactionGroup transactionGroup)
    {
        // TODO: convert to record?
        return new TransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            Amount = transaction.Amount,
            Date = transactionJournal.Date,
            CategoryId = transactionJournal.CategoryId,
            CurrencyId = transaction.CurrencyId,
            Description = transaction.Description,
            ForeignAmount = transaction.ForeignAmount,
            ForeignCurrencyId = transaction.ForeignCurrencyId,
            Reconciled = transaction.Reconciled,
            Source = transaction.IsSource,
            UserId = transaction.UserId,
            SplitTransactionDescription = transactionGroup.Title,
            TransactionGroupId = transactionGroup.Id,
            TransactionJournalId = transaction.TransactionJournalId,
            TransactionTypeId = transactionJournal.TransactionTypeId
        };
    }

    public static TransactionJournalDto ToDto(this TransactionJournal transactionJournal)
    {
        // TODO: Populate the rest
        return new TransactionJournalDto(
            transactionJournal.Id);
    }

    public static TransactionGroupDto ToDto(this TransactionGroup transactionGroup)
    {
        // TODO: Populate the rest
        return new TransactionGroupDto(
            transactionGroup.Id,
            transactionGroup.Title);
    }
}
