FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/ShortTerm/ShortTerm.API/ShortTerm.API.csproj", "src/services/ShortTerm/ShortTerm.API/"]
COPY ["src/services/ShortTerm/ShortTerm.Infrastructure/ShortTerm.Infrastructure.csproj", "src/services/ShortTerm/ShortTerm.Infrastructure/"]
COPY ["src/services/ShortTerm/ShortTerm.Application/ShortTerm.Application.csproj", "src/services/ShortTerm/ShortTerm.Application/"]
COPY ["src/services/ShortTerm/ShortTerm.Domain/ShortTerm.Domain.csproj", "src/services/ShortTerm/ShortTerm.Domain/"]
COPY ["src/services/Customer/Customer.Domain/Customer.Domain.csproj", "src/services/Customer/Customer.Domain/"]
COPY ["src/shared/Shared.Authentication/Shared.Authentication.csproj", "src/shared/Shared.Authentication/"]
COPY ["src/shared/Shared.Logging/Shared.Logging.csproj", "src/shared/Shared.Logging/"]
COPY ["src/shared/Shared.Common/Shared.Common.csproj", "src/shared/Shared.Common/"]
COPY ["src/shared/Shared.Contracts/Shared.Contracts.csproj", "src/shared/Shared.Contracts/"]
COPY ["src/shared/Shared.Messaging/Shared.Messaging.csproj", "src/shared/Shared.Messaging/"]
RUN dotnet restore "./src/services/ShortTerm/ShortTerm.API/ShortTerm.API.csproj"
COPY src/ src/
WORKDIR "/src/src/services/ShortTerm/ShortTerm.API"
RUN dotnet build "./ShortTerm.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShortTerm.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortTerm.API.dll"]
