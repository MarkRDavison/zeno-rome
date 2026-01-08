namespace mark.davison.rome.shared.accounting.constants;

public static class TransactionTypeConstants
{
    public static Guid Withdrawal = new Guid("42E893A8-AA6A-4878-A884-AE610FD184E7");
    public static Guid Deposit = new Guid("A4131938-501C-4BC5-9C7F-0D47A6B11FD6");
    public static Guid Transfer = new Guid("5173AAD0-739B-45F7-9655-1DD8E6162B39");
    public static Guid OpeningBalance = new Guid("9A18E965-3F59-411E-8AD2-5D384ACA61F1");
    public static Guid Reconciliation = new Guid("8F2F67C5-AE8E-4B56-8D89-701618D7EF5F");
    public static Guid Invalid = new Guid("A464337F-8CA1-4470-9535-8C017F88927E");
    public static Guid LiabilityCredit = new Guid("08B600FF-58D7-4D4E-BA2C-C1FE1BA9F972");

    public static IList<Guid> Ids =>
    [
        Withdrawal, Deposit, Transfer, OpeningBalance, Reconciliation, Invalid, LiabilityCredit
    ];
}