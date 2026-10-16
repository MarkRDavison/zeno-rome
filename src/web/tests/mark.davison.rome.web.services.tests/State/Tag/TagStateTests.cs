namespace mark.davison.rome.web.services.tests.State.Tag;

public sealed class TagStateTests
{
    private readonly Mock<IClientHttpRepository> _clientRepository;

    public TagStateTests()
    {
        _clientRepository = new(MockBehavior.Strict);
    }

    [Test]
    public async Task FetchState_WhenResponseSuccessful_PopulatesTags()
    {
        var tag = new TagDto(Guid.NewGuid(), "Holiday");
        _clientRepository
            .Setup(_ => _.Get<TagListQueryRequest, TagListQueryResponse>(CancellationToken.None))
            .ReturnsAsync(new TagListQueryResponse { Value = [tag] });

        var tagState = new TagState(_clientRepository.Object);

        await tagState.FetchState();

        await Assert.That(tagState.Loaded).IsTrue();
        await Assert.That(tagState.Loading).IsFalse();
        await Assert.That(tagState.Tags).Count().IsEqualTo(1);
        await Assert.That(tagState.Tags[0].Id).IsEqualTo(tag.Id);
        await Assert.That(tagState.Tags[0].Name).IsEqualTo(tag.Name);
    }

    [Test]
    public async Task FetchState_WhenResponseFails_LeavesTagsEmpty()
    {
        _clientRepository
            .Setup(_ => _.Get<TagListQueryRequest, TagListQueryResponse>(CancellationToken.None))
            .ReturnsAsync(new TagListQueryResponse { Errors = ["failed"] });

        var tagState = new TagState(_clientRepository.Object);

        await tagState.FetchState();

        await Assert.That(tagState.Loaded).IsTrue();
        await Assert.That(tagState.Loading).IsFalse();
        await Assert.That(tagState.Tags).IsEmpty();
    }

    [Test]
    public async Task FetchState_NotifiesStateChanged()
    {
        _clientRepository
            .Setup(_ => _.Get<TagListQueryRequest, TagListQueryResponse>(CancellationToken.None))
            .ReturnsAsync(new TagListQueryResponse { Value = [] });

        var tagState = new TagState(_clientRepository.Object);
        var changeCount = 0;
        tagState.StateChanged += (_, _) => changeCount++;

        await tagState.FetchState();

        await Assert.That(changeCount).IsGreaterThanOrEqualTo(2);
    }
}
