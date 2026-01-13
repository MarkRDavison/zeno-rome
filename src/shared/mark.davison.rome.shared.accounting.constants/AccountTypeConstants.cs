namespace mark.davison.rome.shared.accounting.constants;

public static class AccountTypeConstants
{
    public static Guid Default = new Guid("8C5DE266-C026-4550-A24B-F79CE91A529A");
    public static Guid Cash = new Guid("FF539008-6CA9-44ED-8F99-ADD8507D1A56");
    public static Guid Asset = new Guid("85579DCA-8679-4F39-B1AD-892C1FC05030");
    public static Guid Expense = new Guid("A1B6D143-240B-48FF-BC1B-34E39FE93D0F");
    public static Guid Revenue = new Guid("4D64ACB4-3870-4339-A587-DD2310897F36");
    public static Guid InitialBalance = new Guid("874AD7A8-EFD8-4D2A-8721-D00FB02C1590");
    public static Guid Beneficiary = new Guid("145931F2-3240-4796-AA41-6520EE7BE9A3");
    public static Guid Import = new Guid("678F2C87-FAB7-4B6F-887A-3C3596169FAB");
    public static Guid Loan = new Guid("C58DE76B-81A0-4FA8-A943-3BC4E71B72A3");
    public static Guid Reconciliation = new Guid("9E3655EE-D6FE-40FB-AB2E-36199763CE6B");
    public static Guid Debt = new Guid("70995D68-F6D2-4002-8D67-FBED3BA8808E");
    public static Guid Mortgage = new Guid("4DB9B14B-447A-4BB5-870E-C0A5EEC2CA63");
    public static Guid LiabilityCredit = new Guid("EB98F6A3-EE9E-4EFD-AA09-8D20E96FEE80");

    public static IEnumerable<Guid> All => [
        Default ,
        Cash,
        Asset,
        Expense,
        Revenue,
        InitialBalance,
        Beneficiary,
        Import,
        Loan,
        Reconciliation,
        Debt,
        Mortgage,
        LiabilityCredit
    ];


    public static IEnumerable<Guid> Assets => [
        Asset
    ];

    public static IEnumerable<Guid> Expenses => [
        Expense
    ];

    public static IEnumerable<Guid> Revenues => [
        Revenue
    ];

    public static IEnumerable<Guid> Liabilities => [
        Loan,
        Debt,
        Mortgage
    ];


    public static IEnumerable<Guid> NonAssetsOrLiabilities => All.Except(Assets.Union(Liabilities));
    public static IEnumerable<Guid> NonRevenues => All.Except(Revenues);
}