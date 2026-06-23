namespace mark.davison.rome.web.components.Utilities;

public static class AmountColourUtilities
{
    public static string GetAmountColourClass(decimal amount)
    {
        Console.WriteLine("GetAmountColourClass: amount: {0}", amount);

        if (amount < 0)
        {
            return "red-numeric-amount";
        }

        return "green-numeric-amount";
    }
}
