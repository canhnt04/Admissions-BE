FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/Driving/Driving.API/Driving.API.csproj", "src/services/Driving/Driving.API/"]
COPY ["src/services/Driving/Driving.Infrastructure/Driving.Infrastructure.csproj", "src/services/Driving/Driving.Infrastructure/"]
COPY ["src/services/Driving/Driving.Application/Driving.Application.csproj", "src/services/Driving/Driving.Application/"]
COPY ["src/services/Driving/Driving.Domain/Driving.Domain.csproj", "src/services/Driving/Driving.Domain/"]
COPY ["src/services/Customer/Customer.Domain/Customer.Domain.csproj", "src/services/Customer/Customer.Domain/"]
COPY ["src/shared/Shared.Authentication/Shared.Authentication.csproj", "src/shared/Shared.Authentication/"]
COPY ["src/shared/Shared.Logging/Shared.Logging.csproj", "src/shared/Shared.Logging/"]
COPY ["src/shared/Shared.Common/Shared.Common.csproj", "src/shared/Shared.Common/"]
COPY ["src/shared/Shared.Contracts/Shared.Contracts.csproj", "src/shared/Shared.Contracts/"]
COPY ["src/shared/Shared.Messaging/Shared.Messaging.csproj", "src/shared/Shared.Messaging/"]
RUN dotnet restore "./src/services/Driving/Driving.API/Driving.API.csproj"
COPY src/ src/
WORKDIR "/src/src/services/Driving/Driving.API"
RUN dotnet build "./Driving.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Driving.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Driving.API.dll"]
