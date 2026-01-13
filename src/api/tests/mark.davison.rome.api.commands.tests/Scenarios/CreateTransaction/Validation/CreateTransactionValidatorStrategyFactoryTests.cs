namespace mark.davison.rome.api.commands.tests.Scenarios.CreateTransaction.Validation;

public sealed class CreateTransactionValidatorStrategyFactoryTests
{
    private readonly CreateTransactionValidatorStrategyFactory _factory;

    public CreateTransactionValidatorStrategyFactoryTests()
    {
        _factory = new();
    }

    [Test]
    public async Task CreateStrategy_UsingWithdrawalTransactionTypeId_CreatesWithdrawalStrategy()
    {
        var strategy = _factory.CreateStrategy(TransactionTypeConstants.Withdrawal);

        await Assert.That(strategy).IsTypeOf<CreateWithdrawalTransactionValidatorStrategy>();
    }

    [Test]
    public async Task CreateStrategy_UsingDepositTransactionTypeId_CreatesDepositStrategy()
    {
        var strategy = _factory.CreateStrategy(TransactionTypeConstants.Deposit);

        await Assert.That(strategy).IsTypeOf<CreateDepositTransactionValidatorStrategy>();
    }

    [Test]
    public async Task CreateStrategy_UsingTransferTransactionTypeId_CreatesTransferStrategy()
    {
        var strategy = _factory.CreateStrategy(TransactionTypeConstants.Transfer);

        await Assert.That(strategy).IsTypeOf<CreateTransferTransactionValidatorStrategy>();
    }

    [Test]
    public async Task CreateStrategy_UsingInvalidTransactionTypeId_CreatesNullStrategy()
    {
        var strategy = _factory.CreateStrategy(Guid.Empty);

        await Assert.That(strategy).IsTypeOf<NullCreateTransactionValidatorStrategy>();
    }
}
