namespace mark.davison.rome.api.Configuration;

public class ApiAppSettings : IRootAppSettings
{
    public string SECTION => "ROME";
    public bool PRODUCTION_MODE { get; set; }
    public AuthenticationSettings AUTHENTICATION { get; set; } = new();
    public RedisSettings REDIS { get; set; } = new();
    public DatabaseAppSettings DATABASE { get; set; } = new();
}
