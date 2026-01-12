namespace mark.davison.rome.shared.accounting.rules.test;

public sealed class DateRulesTests
{

    [Test]
    public async Task GetMonthRange_WorksAsExpected_ForDateInMiddleOfMonth()
    {
        var date = new DateOnly(2022, 3, 15);
        var (start, end) = date.GetMonthRange();

        await Assert.That(start).IsEqualTo(new DateOnly(2022, 3, 1));
        await Assert.That(end).IsEqualTo(new DateOnly(2022, 3, 31));
    }

    [Test]
    public async Task GetMonthRange_WorksAsExpected_ForDateAtStartOfMonth()
    {
        var date = new DateOnly(2022, 3, 1);
        var (start, end) = date.GetMonthRange();

        await Assert.That(start).IsEqualTo(new DateOnly(2022, 3, 1));
        await Assert.That(end).IsEqualTo(new DateOnly(2022, 3, 31));
    }

    [Test]
    public async Task GetMonthRange_WorksAsExpected_ForDateAtEndOfMonth()
    {
        var date = new DateOnly(2022, 3, 31);
        var (start, end) = date.GetMonthRange();

        await Assert.That(start).IsEqualTo(new DateOnly(2022, 3, 1));
        await Assert.That(end).IsEqualTo(new DateOnly(2022, 3, 31));
    }

    [Test]
    public async Task GetMonthRange_WorksAsExpected_ForDateInJanuary()
    {
        var date = new DateOnly(2022, 1, 31);
        var (start, end) = date.GetMonthRange();

        await Assert.That(start).IsEqualTo(new DateOnly(2022, 1, 1));
        await Assert.That(end).IsEqualTo(new DateOnly(2022, 1, 31));
    }

    [Test]
    public async Task GetMonthRange_WorksAsExpected_ForDateInDecember()
    {
        var date = new DateOnly(2022, 12, 31);
        var (start, end) = date.GetMonthRange();

        await Assert.That(start).IsEqualTo(new DateOnly(2022, 12, 1));
        await Assert.That(end).IsEqualTo(new DateOnly(2022, 12, 31));
    }
}
