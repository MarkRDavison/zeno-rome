namespace mark.davison.rome.api.commands.Scenarios.CreateTransaction.Validation;

internal sealed class CreateWithdrawalTransactionValidatorStrategy : CreateTransactionValidatorStrategy
{
    protected override Guid TransactionTypeId => TransactionTypeConstants.Withdrawal;
    protected override IEnumerable<Guid> ValidSourceIds => AccountTypeConstants.Assets.Concat(AccountTypeConstants.Liabilities);
    protected override IEnumerable<Guid> ValidDestinationIds => AccountTypeConstants.Expenses.Concat(AccountTypeConstants.Liabilities);
}