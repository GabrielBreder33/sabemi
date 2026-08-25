FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Sabemi.Payment.Api/Sabemi.Payment.Api.csproj
RUN dotnet publish src/Sabemi.Payment.Api/Sabemi.Payment.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Sabemi.Payment.Api.dll"]
