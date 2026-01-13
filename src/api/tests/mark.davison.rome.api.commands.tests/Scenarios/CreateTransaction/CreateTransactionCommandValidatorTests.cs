namespace mark.davison.rome.api.commands.tests.Scenarios.CreateTransaction;

public class CreateTransactionCommandValidatorTests
{
    private readonly Mock<ICreateTransctionValidationContext> _createTransctionValidationContext;
    private readonly Mock<ICreateTransactionValidatorStrategyFactory> _createTransactionValidatorStrategyFactory;
    private readonly Mock<ICreateTransactionValidatorStrategy> _createTransactionValidatorStrategy;
    private readonly Mock<ICurrentUserContext> _currentUserContext;
    private readonly CreateTransactionCommandValidator _validator;

    public CreateTransactionCommandValidatorTests()
    {
        _createTransctionValidationContext = new(MockBehavior.Strict);
        _createTransactionValidatorStrategyFactory = new(MockBehavior.Strict);
        _createTransactionValidatorStrategy = new(MockBehavior.Strict);
        _currentUserContext = new(MockBehavior.Strict);

        _validator = new(_createTransactionValidatorStrategyFactory.Object, _createTransctionValidationContext.Object);
    }

    [Before(Test)]
    public void TestInitialize()
    {
        _createTransactionValidatorStrategyFactory.Setup(_ => _.CreateStrategy(It.IsAny<Guid>())).Returns(_createTransactionValidatorStrategy.Object);
        _createTransactionValidatorStrategy.Setup(_ => _.ValidateTransactionGroup(It.IsAny<CreateTransactionCommandRequest>(), It.IsAny<CreateTransactionCommandResponse>(), It.IsAny<ICreateTransctionValidationContext>())).Returns(Task.CompletedTask);
        _createTransactionValidatorStrategy.Setup(_ => _.ValidateTranasction(It.IsAny<CreateTransactionDto>(), It.IsAny<CreateTransactionCommandResponse>(), It.IsAny<ICreateTransctionValidationContext>())).Returns(Task.CompletedTask);
    }

    [Test]
    public async Task Validate_ReturnsMessageWhenDateIsInvalid()
    {
        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions = new()
            {
                transaction
            }
        };

        var response = await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(string.Format(CreateTransactionCommandValidator.VALIDATION_DATE, transaction.Id));
    }

    [Test]
    public async Task Validate_WhereMultipleTransactions_ReturnsMessageWhenSplitDescriptionMissing()
    {
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            [
                new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null),
                new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null)
            ]
        };

        var response = await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(CreateTransactionCommandValidator.VALIDATION_GROUP_DESCRIPTION);

    }

    [Test]
    public async Task Validate_ReturnsMessageWhenSourceAndDestinationAccountAreSame()
    {
        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            [
                transaction
            ]
        };

        var response = await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(string.Format(CreateTransactionCommandValidator.VALIDATION_DATE, transaction.Id));
    }

    [Test]
    public async Task Validate_ReturnsMessageWhenTransactionTypeIsInvalid()
    {
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            [
                new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null)
            ]
        };

        var response = await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(CreateTransactionCommandValidator.VALIDATION_TRANSACTION_TYPE);
    }

    [Test]
    public async Task Validate_ReturnsMessageWhenCurrencyIdIsInvalid()
    {
        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            [
                transaction
            ]
        };

        var response = await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(string.Format(CreateTransactionCommandValidator.VALIDATION_CURRENCY_ID, transaction.Id));
    }

    [Test]
    public async Task Validate_ReturnsMessageWhenForeginCurrencyIdIsInvalid()
    {
        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, Guid.Empty);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            [
                transaction
            ]
        };

        var response = await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(string.Format(CreateTransactionCommandValidator.VALIDATION_FOREIGN_CURRENCY_ID, transaction.Id));
    }

    [Test]
    public async Task Validate_ReturnsMessageWhenCurrencyIdsAreSame()
    {
        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, CurrencyConstants.NZD, CurrencyConstants.NZD);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            [
                transaction
            ]
        };

        var response = await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Contains(string.Format(CreateTransactionCommandValidator.VALIDATION_DUPLICATE_CURRENCY, transaction.Id));
    }

    [Test]
    public async Task Validate_CallsValidateTransactionGroup_OnValidatorStrategy()
    {

        _createTransactionValidatorStrategy
            .Setup(_ => _.ValidateTransactionGroup(
                It.IsAny<CreateTransactionCommandRequest>(),
                It.IsAny<CreateTransactionCommandResponse>(),
                It.IsAny<ICreateTransctionValidationContext>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        _createTransactionValidatorStrategy
            .Setup(_ => _.ValidateTranasction(
                It.IsAny<CreateTransactionDto>(),
                It.IsAny<CreateTransactionCommandResponse>(),
                It.IsAny<ICreateTransctionValidationContext>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            [
                new CreateTransactionDto(Guid.NewGuid(), string.Empty, Guid.Empty, Guid.Empty, DateOnly.MinValue, 0, null, Guid.Empty, null)
            ]
        };

        await _validator.ValidateAsync(request, _currentUserContext.Object, CancellationToken.None);

        _createTransactionValidatorStrategy
            .Verify(_ => _.ValidateTransactionGroup(
                It.IsAny<CreateTransactionCommandRequest>(),
                It.IsAny<CreateTransactionCommandResponse>(),
                It.IsAny<ICreateTransctionValidationContext>()),
            Times.Once);
        _createTransactionValidatorStrategy
            .Verify(_ => _.ValidateTranasction(
                It.IsAny<CreateTransactionDto>(),
                It.IsAny<CreateTransactionCommandResponse>(),
                It.IsAny<ICreateTransctionValidationContext>()),
            Times.Once);
    }
}
