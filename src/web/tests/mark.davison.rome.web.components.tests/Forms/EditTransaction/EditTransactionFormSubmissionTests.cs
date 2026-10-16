using mark.davison.rome.shared.models.dto.Scenarios.Commands.CreateTransaction;

namespace mark.davison.rome.web.components.tests.Forms.EditTransaction;

public sealed class EditTransactionFormSubmissionTests
{
    [Test]
    public async Task Primary_IncludesTagIdsOnCreateTransactionRequest()
    {
        var tagId = Guid.NewGuid();
        var currencyId = Guid.NewGuid();
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();

        var accountState = new Mock<IAccountState>();
        accountState.Setup(_ => _.Accounts).Returns([
            new AccountDto(
                sourceAccountId,
                "Source",
                Guid.Empty,
                string.Empty,
                0,
                true,
                DateTimeOffset.UtcNow,
                0,
                currencyId,
                null,
                null,
                null)
        ]);

        CreateTransactionCommandRequest? capturedRequest = null;
        var client = new Mock<IClientHttpRepository>();
        client
            .Setup(_ => _.Post<CreateTransactionCommandRequest, CreateTransactionCommandResponse>(It.IsAny<CreateTransactionCommandRequest>(), It.IsAny<CancellationToken>()))
            .Callback<CreateTransactionCommandRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new CreateTransactionCommandResponse { Group = new TransactionGroupDto(Guid.NewGuid(), string.Empty) });

        var submission = new EditTransactionFormSubmission(accountState.Object, client.Object);

        var startupState = new Mock<IStartupState>();
        var categoryState = new Mock<ICategoryState>();
        categoryState.Setup(_ => _.Categories).Returns([]);
        var tagState = new Mock<ITagState>();
        tagState.Setup(_ => _.Tags).Returns([]);

        var formViewModel = new EditTransactionFormViewModel(
            startupState.Object,
            accountState.Object,
            categoryState.Object,
            tagState.Object)
        {
            TransactionTypeId = TransactionTypeConstants.Withdrawal,
            Date = DateTime.Today,
            Items =
            {
                new EditTransactionFormViewModelItem
                {
                    Id = Guid.NewGuid(),
                    Description = "Test",
                    SourceAccountId = sourceAccountId,
                    DestinationAccountId = destinationAccountId,
                    Amount = 10,
                    CategoryId = Guid.NewGuid(),
                    TagIds = [tagId]
                }
            }
        };

        await submission.Primary(formViewModel);

        await Assert.That(capturedRequest).IsNotNull();
        await Assert.That(capturedRequest!.Transactions[0].TagIds).Contains(tagId);
    }
}
