namespace mark.davison.rome.web.playwright.tests;

#if SKIP_TUNIT_TESTS
public abstract class RomeBaseTest : SkipBaseTest
#else
public abstract class RomeBaseTest : LoggedInTest
#endif
{

    public RomeBaseTest() : base(DataNames.Instance)
    {
    }
}
