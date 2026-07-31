FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/services/LeadAssignment/LeadAssignment.API/LeadAssignment.API.csproj", "src/services/LeadAssignment/LeadAssignment.API/"]
COPY ["src/services/LeadAssignment/LeadAssignment.Infrastructure/LeadAssignment.Infrastructure.csproj", "src/services/LeadAssignment/LeadAssignment.Infrastructure/"]
COPY ["src/services/LeadAssignment/LeadAssignment.Application/LeadAssignment.Application.csproj", "src/services/LeadAssignment/LeadAssignment.Application/"]
COPY ["src/services/LeadAssignment/LeadAssignment.Domain/LeadAssignment.Domain.csproj", "src/services/LeadAssignment/LeadAssignment.Domain/"]
COPY ["src/shared/Shared.Common/Shared.Common.csproj", "src/shared/Shared.Common/"]
COPY ["src/shared/Shared.Messaging/Shared.Messaging.csproj", "src/shared/Shared.Messaging/"]
COPY ["src/shared/Shared.Contracts/Shared.Contracts.csproj", "src/shared/Shared.Contracts/"]
RUN dotnet restore "./src/services/LeadAssignment/LeadAssignment.API/LeadAssignment.API.csproj"
COPY src/ src/
WORKDIR "/src/src/services/LeadAssignment/LeadAssignment.API"
RUN dotnet build "./LeadAssignment.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LeadAssignment.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeadAssignment.API.dll"]
