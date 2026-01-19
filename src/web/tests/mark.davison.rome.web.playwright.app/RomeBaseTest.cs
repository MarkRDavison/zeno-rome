namespace mark.davison.rome.web.playwright.app;

#if SKIP_TUNIT_TESTS
public abstract class RomeBaseTest : SkipBaseTest
#else
public abstract class RomeBaseTest : LoggedInTest
#endif
{

    public RomeBaseTest() : base(DataNames.Instance)
    {
    }

    protected string MakeAccountNumber() => _faker.Finance.Account();
    protected string MakeAccountName() => _faker.Finance.AccountName();
    protected DashboardPage Dashboard => new(CurrentPage, AppSettings);
}
