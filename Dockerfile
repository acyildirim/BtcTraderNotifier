FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 5020

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore "src/BtcTrader/BtcTrader.csproj"

# copy everything else and build
COPY . ./
FROM build AS publish

RUN dotnet publish src/BtcTrader/BtcTrader.csproj -c Release -o /app/publish



FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BtcTrader.dll"]




