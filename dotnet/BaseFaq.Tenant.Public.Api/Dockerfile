FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5004
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet/BaseFaq.Tenant.Public.Api/BaseFaq.Tenant.Public.Api.csproj", "dotnet/BaseFaq.Tenant.Public.Api/"]
RUN dotnet restore "dotnet/BaseFaq.Tenant.Public.Api/BaseFaq.Tenant.Public.Api.csproj"
COPY . .
WORKDIR "/src/dotnet/BaseFaq.Tenant.Public.Api"
RUN dotnet build "./BaseFaq.Tenant.Public.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BaseFaq.Tenant.Public.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaseFaq.Tenant.Public.Api.dll"]
