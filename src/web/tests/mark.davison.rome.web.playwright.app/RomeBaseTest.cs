namespace mark.davison.rome.web.playwright.app;

// TODO: Common interface with all properties that are shared between SkipBaseTest and LoggedInTest
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

#if SKIP_TUNIT_TESTS

    [Before(Test)]
    public void BeforeTest()
    {
        Skip.Test("Dont run ui tests from main solution");
    }

    protected DashboardPage Dashboard => throw new NotImplementedException();
#else
    protected DashboardPage Dashboard => new(CurrentPage, AppSettings);
#endif
}
