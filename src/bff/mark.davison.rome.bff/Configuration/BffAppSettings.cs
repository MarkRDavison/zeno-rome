namespace mark.davison.rome.bff.Configuration;

public class BffAppSettings : IRootAppSettings
{
    public string SECTION => "ROME";
    public bool PRODUCTION_MODE { get; set; }
    public AuthenticationSettings AUTHENTICATION { get; set; } = new();
    public RedisSettings REDIS { get; set; } = new();
    public string API_ORIGIN { get; set; } = "https://localhost:50000";
    public string WEB_ORIGIN { get; set; } = "https://localhost:8080";
}