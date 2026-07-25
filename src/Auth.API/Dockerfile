FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Auth.API/Auth.API.csproj", "src/Auth.API/"]
COPY ["src/Crm.Infrastructure/Crm.Infrastructure.csproj", "src/Crm.Infrastructure/"]
COPY ["src/Crm.Application/Crm.Application.csproj", "src/Crm.Application/"]
COPY ["src/Crm.Domain/Crm.Domain.csproj", "src/Crm.Domain/"]
RUN dotnet restore "./src/Auth.API/Auth.API.csproj"
COPY src/ src/
WORKDIR "/src/src/Auth.API"
RUN dotnet build "./Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]
