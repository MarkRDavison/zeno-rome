using mark.davison.common.authentication.server.abstractions.Services;
using mark.davison.common.persistence;
using mark.davison.common.server.test.Persistence;
using mark.davison.rome.api.commands.Scenarios.UpsertAccount;
using mark.davison.rome.api.models.Entities;
using mark.davison.rome.api.persistence;
using mark.davison.rome.shared.accounting.constants;
using mark.davison.rome.shared.accounting.rules;
using mark.davison.rome.shared.models.dto.Scenarios.Commands.UpsertAccount;
using mark.davison.rome.shared.models.dto.Shared;
using Moq;

namespace mark.davison.rome.api.commands.tests.Scenarios.UpsertAccount;

public sealed class UpsertAccountCommandValidatorTests
{
    private readonly UpsertAccountCommandValidator _upsertAccountCommandValidator;
    private readonly Mock<ICurrentUserContext> _currentUserContext;
    private readonly IDbContext<RomeDbContext> _dbContext;
    private readonly List<AccountType> _accountTypes;
    private readonly List<Currency> _currencies;
    private readonly Guid _userId;

    public UpsertAccountCommandValidatorTests()
    {
        _dbContext = DbContextHelpers.CreateInMemory(_ => new RomeDbContext(_));
        _currentUserContext = new(MockBehavior.Strict);

        _accountTypes = new()
        {
            new AccountType { Id = AccountTypeConstants.Asset, Type = "Asset", UserId = Guid.Empty, Created = DateTime.UtcNow, LastModified = DateTime.UtcNow },
            new AccountType { Id = AccountTypeConstants.Expense, Type = "Expense", UserId = Guid.Empty, Created = DateTime.UtcNow, LastModified = DateTime.UtcNow },
            new AccountType { Id = AccountTypeConstants.Revenue, Type = "Revenue", UserId = Guid.Empty, Created = DateTime.UtcNow, LastModified = DateTime.UtcNow },
            new AccountType { Id = AccountTypeConstants.Cash, Type = "Cash", UserId = Guid.Empty, Created = DateTime.UtcNow, LastModified = DateTime.UtcNow }
        };
        _currencies = new()
        {
            new Currency { Id = CurrencyConstants.NZD, UserId = Guid.Empty, Created = DateTime.UtcNow, LastModified = DateTime.UtcNow }
        };
        _userId = Guid.NewGuid();
        _currentUserContext.Setup(_ => _.UserId).Returns(_userId);

        _upsertAccountCommandValidator = new UpsertAccountCommandValidator(_dbContext);
    }

    [Before(Test)]
    public async Task TestInitialize()
    {
        await _dbContext.UpsertEntitiesAsync(_accountTypes, CancellationToken.None);
        await _dbContext.UpsertEntitiesAsync(_currencies, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
    }

    [Test]
    public async Task Validate_WhereAccountIdIsNotValid_ReturnsError()
    {
        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, string.Empty, null, string.Empty, Guid.Empty, Guid.Empty, null, null)
        };
        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Any(_ => _.Contains(UpsertAccountCommandValidator.VALIDATION_ACCOUNT_TYPE_ID));
    }

    [Test]
    public async Task Validate_WhereCurrencyIdIsNotValid_ReturnsError()
    {
        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, string.Empty, null, string.Empty, AccountTypeConstants.Asset, Guid.Empty, null, null)
        };

        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Any(_ => _.Contains(UpsertAccountCommandValidator.VALIDATION_CURRENCY_ID));
    }

    [Test]
    public async Task Validate_WhereNameIsNotValid_ReturnsError()
    {
        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, string.Empty, null, string.Empty, AccountTypeConstants.Asset, CurrencyConstants.NZD, null, null)
        };
        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Any(_ => _.Contains(string.Format(UpsertAccountCommandValidator.VALIDATION_MISSING_REQ_FIELD, nameof(Account.Name))));
    }

    [Test]
    public async Task Validate_WhereAccountNumberIsDuplicated_WithoutExpenseOrRevenue_ReturnsError()
    {
        const string AccountNumber = "DUPLICATE_NUMBER";

        await _dbContext.UpsertEntityAsync(new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Asset,
            CurrencyId = CurrencyConstants.NZD,
            UserId = _userId,
            AccountNumber = AccountNumber,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, "Name", null, AccountNumber, AccountTypeConstants.Asset, CurrencyConstants.NZD, null, null)
        };

        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Any(_ => _.Contains(UpsertAccountCommandValidator.VALIDATION_DUPLICATE_ACC_NUM));
    }

    [Test]
    public async Task Validate_WhereAccountNumberIsDuplicated_WithExpense_ReturnsSuccess()
    {
        const string AccountNumber = "DUPLICATE_NUMBER";

        await _dbContext.UpsertEntityAsync(new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Revenue,
            CurrencyId = CurrencyConstants.NZD,
            UserId = _userId,
            AccountNumber = AccountNumber,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, "Name", null, AccountNumber, AccountTypeConstants.Expense, CurrencyConstants.NZD, null, null)
        };
        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    [Test]
    public async Task Validate_WhereAccountNumberIsDuplicated_WithRevenue_ReturnsSuccess()
    {
        const string AccountNumber = "DUPLICATE_NUMBER";

        await _dbContext.UpsertEntityAsync(new Account
        {
            Id = Guid.NewGuid(),
            AccountTypeId = AccountTypeConstants.Expense,
            CurrencyId = CurrencyConstants.NZD,
            UserId = _userId,
            AccountNumber = AccountNumber,
            Created = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        }, CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, "Name", null, AccountNumber, AccountTypeConstants.Revenue, CurrencyConstants.NZD, null, null)
        };
        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    [Test]
    public async Task Validate_WhereAccountNumberIsDuplicatedAgainstMultiple_WithRevenue_ReturnsSuccess()
    {
        const string AccountNumber = "DUPLICATE_NUMBER";

        await _dbContext.UpsertEntitiesAsync(
            [
                new Account
                {
                    Id = Guid.NewGuid(),
                    AccountTypeId = AccountTypeConstants.Asset,
                    CurrencyId = CurrencyConstants.NZD,
                    UserId = _userId,
                    AccountNumber = AccountNumber,
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    AccountTypeId = AccountTypeConstants.Asset,
                    CurrencyId = CurrencyConstants.NZD,
                    UserId = _userId,
                    AccountNumber = AccountNumber,
                    Created = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow
                }
            ],
            CancellationToken.None);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, "Name", null, AccountNumber, AccountTypeConstants.Revenue, CurrencyConstants.NZD, null, null)
        };
        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Any(_ => _.Contains(UpsertAccountCommandValidator.VALIDATION_DUPLICATE_ACC_NUM));
    }

    [Test]
    public async Task Validate_WhereAccountNumberIsNotDuplicated_ReturnsSuccess()
    {
        const string AccountNumber = "DUPLICATE_NUMBER";

        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, "Name", null, AccountNumber, AccountTypeConstants.Asset, CurrencyConstants.NZD, null, null)
        };

        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsTrue();
    }

    [Test]
    public async Task Validate_WhereOpeningBalanceSpecifiedButNotOpeningBalanceDate_ReturnsError()
    {
        var request = new UpsertAccountCommandRequest
        {
            UpsertAccountDto = new UpsertAccountDto(Guid.Empty, "Name", null, "AccountNumber", AccountTypeConstants.Asset, CurrencyConstants.NZD, CurrencyRules.ToPersisted(100.0M), null)
        };

        var response = await _upsertAccountCommandValidator.ValidateAsync(
            request,
            _currentUserContext.Object,
            CancellationToken.None);

        await Assert.That(response.Success).IsFalse();
        await Assert.That(response.Errors).Any(_ => _.Contains(UpsertAccountCommandValidator.VALIDATION_MISSING_OPENING_BAL_DATE));
    }
}

