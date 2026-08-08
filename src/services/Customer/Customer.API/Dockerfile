FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/Customer/Customer.API/Customer.API.csproj", "src/services/Customer/Customer.API/"]
COPY ["src/services/Customer/Customer.Application/Customer.Application.csproj", "src/services/Customer/Customer.Application/"]
COPY ["src/services/Customer/Customer.Infrastructure/Customer.Infrastructure.csproj", "src/services/Customer/Customer.Infrastructure/"]
COPY ["src/services/Customer/Customer.Domain/Customer.Domain.csproj", "src/services/Customer/Customer.Domain/"]
COPY ["src/shared/Shared.Authentication/Shared.Authentication.csproj", "src/shared/Shared.Authentication/"]
COPY ["src/shared/Shared.Common/Shared.Common.csproj", "src/shared/Shared.Common/"]
COPY ["src/shared/Shared.Contracts/Shared.Contracts.csproj", "src/shared/Shared.Contracts/"]
RUN dotnet restore "./src/services/Customer/Customer.API/Customer.API.csproj"
COPY src/ src/
WORKDIR "/src/src/services/Customer/Customer.API"
RUN dotnet build "./Customer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Customer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Customer.API.dll"]
