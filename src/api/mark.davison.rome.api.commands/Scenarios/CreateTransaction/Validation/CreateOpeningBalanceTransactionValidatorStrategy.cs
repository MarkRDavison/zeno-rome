namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

internal sealed class CreateOpeningBalanceTransactionValidatorStrategy : CreateTransactionValidatorStrategy
{
    protected override Guid TransactionTypeId => TransactionTypeConstants.OpeningBalance;
    protected override IEnumerable<Guid> ValidSourceIds => [AccountTypeConstants.InitialBalance];
    protected override IEnumerable<Guid> ValidDestinationIds =>
        AccountTypeConstants.All.Except([
            AccountTypeConstants.InitialBalance,
            AccountTypeConstants.Reconciliation,
            AccountTypeConstants.Import,
        ]);
}
