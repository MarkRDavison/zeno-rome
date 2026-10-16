namespace mark.davison.rome.shared.accounting.rules.test;

public sealed class AllowableSourceDestinationAccountsOpeningBalanceTests
{
    [Test]
    public async Task OpeningBalance_InitialBalanceToAsset_IsValid()
    {
        var valid = AllowableSourceDestinationAccounts.IsValidSourceAndDestinationAccount(
            TransactionTypeConstants.OpeningBalance,
            AccountTypeConstants.InitialBalance,
            AccountTypeConstants.Asset);

        await Assert.That(valid).IsTrue();
    }

    [Test]
    public async Task OpeningBalance_InitialBalanceToExpense_IsValid()
    {
        var valid = AllowableSourceDestinationAccounts.IsValidSourceAndDestinationAccount(
            TransactionTypeConstants.OpeningBalance,
            AccountTypeConstants.InitialBalance,
            AccountTypeConstants.Expense);

        await Assert.That(valid).IsTrue();
    }

    [Test]
    public async Task OpeningBalance_InitialBalanceToRevenue_IsValid()
    {
        var valid = AllowableSourceDestinationAccounts.IsValidSourceAndDestinationAccount(
            TransactionTypeConstants.OpeningBalance,
            AccountTypeConstants.InitialBalance,
            AccountTypeConstants.Revenue);

        await Assert.That(valid).IsTrue();
    }
}
