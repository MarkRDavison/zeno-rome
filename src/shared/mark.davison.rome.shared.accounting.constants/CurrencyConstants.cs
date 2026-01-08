namespace mark.davison.rome.shared.accounting.constants;

public static class CurrencyConstants
{
    public static Guid NZD = new Guid("746249E4-2DFA-483A-862F-6A9D4A86BC69");
    public static Guid AUD = new Guid("D6F10E8D-9EDE-4A5C-8EF7-A3CC3E5512FF");
    public static Guid USD = new Guid("220193B4-A2D4-4008-8BD4-7E5CF46A544C");
    public static Guid CAD = new Guid("C89F1D5B-65FF-4B99-9365-1C1E1CEA5EEF");
    public static Guid EUR = new Guid("E8C0A331-5BD2-4051-87D5-C6D0320D599A");
    public static Guid GBP = new Guid("D78CF1FC-9D08-4A71-8A5B-91D4407327CA");
    public static Guid JPY = new Guid("E63BF389-D189-46B1-B5DC-A6594D7A91C2");
    public static Guid RMB = new Guid("62812AF7-2079-4D61-BE2A-D320AFE938BF");
    public static Guid INT = new Guid("F9267ABE-C720-4645-A25F-FBDF1E2EB0BA");

    public static IList<Guid> Ids => new List<Guid>
    {
        NZD,
        AUD,
        USD,
        CAD,
        EUR,
        GBP,
        JPY,
        RMB
    };
}