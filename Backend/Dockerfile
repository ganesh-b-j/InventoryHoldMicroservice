FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/InventoryHold.Contracts/InventoryHold.Contracts.csproj ./InventoryHold.Contracts/
COPY src/InventoryHold.Domain/InventoryHold.Domain.csproj ./InventoryHold.Domain/
COPY src/InventoryHold.Infrastructure/InventoryHold.Infrastructure.csproj ./InventoryHold.Infrastructure/
COPY src/InventoryHold.WebApi/InventoryHold.WebApi.csproj ./InventoryHold.WebApi/
COPY src/InventoryHold.UnitTests/InventoryHold.UnitTests.csproj ./InventoryHold.UnitTests/

RUN dotnet restore ./InventoryHold.WebApi/InventoryHold.WebApi.csproj

COPY src/ ./
WORKDIR /src/InventoryHold.WebApi
RUN dotnet publish InventoryHold.WebApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InventoryHold.WebApi.dll"]
