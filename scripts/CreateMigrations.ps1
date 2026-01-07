 param(
    [Parameter(Mandatory = $true)]
    [string]$MigrationName
)
 
 dotnet ef migrations add $MigrationName --project ..\src\api\migrations\mark.davison.rome.api.migrations.sqlite\mark.davison.rome.api.migrations.sqlite.csproj
 dotnet ef migrations add $MigrationName --project ..\src\api\migrations\mark.davison.rome.api.migrations.postgres\mark.davison.rome.api.migrations.postgres.csproj