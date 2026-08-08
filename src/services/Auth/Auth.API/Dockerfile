FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/Auth/Auth.API/Auth.API.csproj", "src/services/Auth/Auth.API/"]
COPY ["src/services/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj", "src/services/Auth/Auth.Infrastructure/"]
COPY ["src/services/Auth/Auth.Application/Auth.Application.csproj", "src/services/Auth/Auth.Application/"]
COPY ["src/services/Auth/Auth.Domain/Auth.Domain.csproj", "src/services/Auth/Auth.Domain/"]
COPY ["src/shared/Shared.Authentication/Shared.Authentication.csproj", "src/shared/Shared.Authentication/"]
COPY ["src/shared/Shared.Logging/Shared.Logging.csproj", "src/shared/Shared.Logging/"]
COPY ["src/shared/Shared.Common/Shared.Common.csproj", "src/shared/Shared.Common/"]
COPY ["src/shared/Shared.Protos/Shared.Protos.csproj", "src/shared/Shared.Protos/"]
COPY ["src/shared/Shared.Contracts/Shared.Contracts.csproj", "src/shared/Shared.Contracts/"]
COPY ["src/shared/Shared.Messaging/Shared.Messaging.csproj", "src/shared/Shared.Messaging/"]
RUN dotnet restore "./src/services/Auth/Auth.API/Auth.API.csproj"
COPY src/ src/
WORKDIR "/src/src/services/Auth/Auth.API"
RUN dotnet build "./Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]
