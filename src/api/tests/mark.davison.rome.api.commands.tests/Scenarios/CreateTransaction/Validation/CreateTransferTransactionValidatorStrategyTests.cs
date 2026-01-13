namespace mark.davison.rome.api.commands.tests.Scenarios.CreateTransaction.Validation;

public sealed class CreateTransferTransactionValidatorStrategyTests
{
    private readonly Mock<ICreateTransctionValidationContext> _createTransctionValidationContext;
    private readonly CreateTransferTransactionValidatorStrategy _strategy;

    public CreateTransferTransactionValidatorStrategyTests()
    {
        _createTransctionValidationContext = new(MockBehavior.Strict);
        _strategy = new();
    }

    [Test]
    public async Task ValidateTransactionGroup_DoesNothing()
    {
        var request = new CreateTransactionCommandRequest();
        var response = new CreateTransactionCommandResponse();

        await _strategy.ValidateTransactionGroup(request, response, _createTransctionValidationContext.Object);

        await Assert.That(response.Errors).IsEmpty();
    }

    [Test]
    [MethodDataSource(typeof(AccountTypeConstants), nameof(AccountTypeConstants.Assets))]
    public async Task ValidateTransaction_PassesForValidDestinationAccount_ForAsset(Guid accountTypeId)
    {
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Asset,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var destinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = accountTypeId,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, sourceAccount.Id, destinationAccount.Id, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            {
                transaction
            }
        };

        var response = new CreateTransactionCommandResponse();

        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);
        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount)
            .Verifiable();

        await _strategy.ValidateTranasction(transaction, response, _createTransctionValidationContext.Object);

        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_SOURCE_ACCOUNT_TYPE);
        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_ACCOUNT_PAIR);

        _createTransctionValidationContext
            .Verify(
                _ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Test]
    [MethodDataSource(typeof(AccountTypeConstants), nameof(AccountTypeConstants.Liabilities))]
    public async Task ValidateTransaction_PassesForValidDestinationAccount_ForLiability(Guid accountTypeId)
    {
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Mortgage,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var destinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = accountTypeId,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, sourceAccount.Id, destinationAccount.Id, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            {
                transaction
            }
        };

        var response = new CreateTransactionCommandResponse();

        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);
        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount)
            .Verifiable();

        await _strategy.ValidateTranasction(transaction, response, _createTransctionValidationContext.Object);

        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_SOURCE_ACCOUNT_TYPE);
        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_ACCOUNT_PAIR);

        _createTransctionValidationContext
            .Verify(
                _ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Test]
    [MethodDataSource(typeof(AccountTypeConstants), nameof(AccountTypeConstants.Revenues))]
    public async Task ValidateTransaction_FailsForInvalidDestinationAccount(Guid accountTypeId)
    {
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Asset,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var destinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = accountTypeId,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, sourceAccount.Id, destinationAccount.Id, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            {
                transaction
            }
        };

        var response = new CreateTransactionCommandResponse();

        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);
        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount)
            .Verifiable();

        await _strategy.ValidateTranasction(transaction, response, _createTransctionValidationContext.Object);

        await Assert.That(response.Errors).Contains(CreateTransactionCommandValidator.VALIDATION_INVALID_DESTINATION_ACCOUNT_TYPE);

        _createTransctionValidationContext
            .Verify(
                _ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Test]
    [MethodDataSource(typeof(AccountTypeConstants), nameof(AccountTypeConstants.Assets))]
    public async Task ValidateTransaction_PassesForValidSourceAccount_ForAsset(Guid accountTypeId)
    {
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = accountTypeId,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var destinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Asset,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, sourceAccount.Id, destinationAccount.Id, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            {
                transaction
            }
        };

        var response = new CreateTransactionCommandResponse();

        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount)
            .Verifiable();
        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        await _strategy.ValidateTranasction(transaction, response, _createTransctionValidationContext.Object);

        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_SOURCE_ACCOUNT_TYPE);
        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_ACCOUNT_PAIR);

        _createTransctionValidationContext
            .Verify(
                _ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Test]
    [MethodDataSource(typeof(AccountTypeConstants), nameof(AccountTypeConstants.Liabilities))]
    public async Task ValidateTransaction_PassesForValidSourceAccount_ForLiability(Guid accountTypeId)
    {
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = accountTypeId,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var destinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Debt,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, sourceAccount.Id, destinationAccount.Id, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            {
                transaction
            }
        };

        var response = new CreateTransactionCommandResponse();

        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount)
            .Verifiable();
        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        await _strategy.ValidateTranasction(transaction, response, _createTransctionValidationContext.Object);

        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_SOURCE_ACCOUNT_TYPE);
        await Assert.That(response.Errors).DoesNotContain(CreateTransactionCommandValidator.VALIDATION_INVALID_ACCOUNT_PAIR);

        _createTransctionValidationContext
            .Verify(
                _ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Test]
    [MethodDataSource(typeof(AccountTypeConstants), nameof(AccountTypeConstants.NonAssetsOrLiabilities))]
    public async Task ValidateTransaction_FailsForInvalidSourceAccount(Guid accountTypeId)
    {
        var sourceAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = accountTypeId,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        var destinationAccount = new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Expense,
            UserId = Guid.Empty,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        var transaction = new CreateTransactionDto(Guid.NewGuid(), string.Empty, sourceAccount.Id, destinationAccount.Id, DateOnly.MinValue, 0, null, Guid.Empty, null);
        var request = new CreateTransactionCommandRequest
        {
            Transactions =
            {
                transaction
            }
        };

        var response = new CreateTransactionCommandResponse();

        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount)
            .Verifiable();
        _createTransctionValidationContext
            .Setup(_ => _.GetAccountById(transaction.DestinationAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        await _strategy.ValidateTranasction(transaction, response, _createTransctionValidationContext.Object);

        await Assert.That(response.Errors).Contains(CreateTransactionCommandValidator.VALIDATION_INVALID_SOURCE_ACCOUNT_TYPE);

        _createTransctionValidationContext
            .Verify(
                _ => _.GetAccountById(transaction.SourceAccountId, It.IsAny<CancellationToken>()),
                Times.Once);
    }
}
