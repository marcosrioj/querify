FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet/Querify.Tenant.Worker.Api/Querify.Tenant.Worker.Api.csproj", "dotnet/Querify.Tenant.Worker.Api/"]
COPY ["dotnet/Querify.Common.Infrastructure.Core/Querify.Common.Infrastructure.Core.csproj", "dotnet/Querify.Common.Infrastructure.Core/"]
COPY ["dotnet/Querify.Common.Infrastructure.Telemetry/Querify.Common.Infrastructure.Telemetry.csproj", "dotnet/Querify.Common.Infrastructure.Telemetry/"]
COPY ["dotnet/Querify.Common.EntityFramework.Tenant/Querify.Common.EntityFramework.Tenant.csproj", "dotnet/Querify.Common.EntityFramework.Tenant/"]
COPY ["dotnet/Querify.Tenant.Worker.Business.Billing/Querify.Tenant.Worker.Business.Billing.csproj", "dotnet/Querify.Tenant.Worker.Business.Billing/"]
COPY ["dotnet/Querify.Tenant.Worker.Business.Email/Querify.Tenant.Worker.Business.Email.csproj", "dotnet/Querify.Tenant.Worker.Business.Email/"]
COPY ["dotnet/Querify.Common.Infrastructure.ApiErrorHandling/Querify.Common.Infrastructure.ApiErrorHandling.csproj", "dotnet/Querify.Common.Infrastructure.ApiErrorHandling/"]
COPY ["dotnet/Querify.Common.Infrastructure.MediatR/Querify.Common.Infrastructure.MediatR.csproj", "dotnet/Querify.Common.Infrastructure.MediatR/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core/Querify.Common.EntityFramework.Core.csproj", "dotnet/Querify.Common.EntityFramework.Core/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core.Audit/Querify.Common.EntityFramework.Core.Audit.csproj", "dotnet/Querify.Common.EntityFramework.Core.Audit/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core.SoftDelete/Querify.Common.EntityFramework.Core.SoftDelete.csproj", "dotnet/Querify.Common.EntityFramework.Core.SoftDelete/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core.Tenant/Querify.Common.EntityFramework.Core.Tenant.csproj", "dotnet/Querify.Common.EntityFramework.Core.Tenant/"]
COPY ["dotnet/Querify.Models.Common/Querify.Models.Common.csproj", "dotnet/Querify.Models.Common/"]
COPY ["dotnet/Querify.Models.Tenant/Querify.Models.Tenant.csproj", "dotnet/Querify.Models.Tenant/"]
COPY ["dotnet/Querify.Models.User/Querify.Models.User.csproj", "dotnet/Querify.Models.User/"]
RUN dotnet restore "dotnet/Querify.Tenant.Worker.Api/Querify.Tenant.Worker.Api.csproj"
COPY . .
WORKDIR "/src/dotnet/Querify.Tenant.Worker.Api"
RUN dotnet build "./Querify.Tenant.Worker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Querify.Tenant.Worker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Querify.Tenant.Worker.Api.dll"]
