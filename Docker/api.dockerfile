FROM mcr.microsoft.com/dotnet/sdk:10.0 as BUILD
WORKDIR /app 

COPY / /app/
RUN dotnet restore ./zeno-rome.slnx
RUN dotnet publish -c Release -o out api/mark.davison.rome.api/mark.davison.rome.api.csproj

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /app
COPY --from=BUILD /app/out .

ENTRYPOINT ["dotnet", "mark.davison.rome.api.dll"]