FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet/BaseFaq.Tenant.Worker.Api/BaseFaq.Tenant.Worker.Api.csproj", "dotnet/BaseFaq.Tenant.Worker.Api/"]
RUN dotnet restore "dotnet/BaseFaq.Tenant.Worker.Api/BaseFaq.Tenant.Worker.Api.csproj"
COPY . .
WORKDIR "/src/dotnet/BaseFaq.Tenant.Worker.Api"
RUN dotnet build "./BaseFaq.Tenant.Worker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BaseFaq.Tenant.Worker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaseFaq.Tenant.Worker.Api.dll"]
