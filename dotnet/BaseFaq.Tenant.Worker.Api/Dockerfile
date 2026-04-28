FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet/BaseFaq.Tenant.Worker.Api/BaseFaq.Tenant.Worker.Api.csproj", "dotnet/BaseFaq.Tenant.Worker.Api/"]
COPY ["dotnet/BaseFaq.Common.Infrastructure.Core/BaseFaq.Common.Infrastructure.Core.csproj", "dotnet/BaseFaq.Common.Infrastructure.Core/"]
COPY ["dotnet/BaseFaq.Common.Infrastructure.Telemetry/BaseFaq.Common.Infrastructure.Telemetry.csproj", "dotnet/BaseFaq.Common.Infrastructure.Telemetry/"]
COPY ["dotnet/BaseFaq.Common.EntityFramework.Tenant/BaseFaq.Common.EntityFramework.Tenant.csproj", "dotnet/BaseFaq.Common.EntityFramework.Tenant/"]
COPY ["dotnet/BaseFaq.Tenant.Worker.Business.Billing/BaseFaq.Tenant.Worker.Business.Billing.csproj", "dotnet/BaseFaq.Tenant.Worker.Business.Billing/"]
COPY ["dotnet/BaseFaq.Tenant.Worker.Business.Email/BaseFaq.Tenant.Worker.Business.Email.csproj", "dotnet/BaseFaq.Tenant.Worker.Business.Email/"]
COPY ["dotnet/BaseFaq.Common.Infrastructure.ApiErrorHandling/BaseFaq.Common.Infrastructure.ApiErrorHandling.csproj", "dotnet/BaseFaq.Common.Infrastructure.ApiErrorHandling/"]
COPY ["dotnet/BaseFaq.Common.Infrastructure.MediatR/BaseFaq.Common.Infrastructure.MediatR.csproj", "dotnet/BaseFaq.Common.Infrastructure.MediatR/"]
COPY ["dotnet/BaseFaq.Common.EntityFramework.Core/BaseFaq.Common.EntityFramework.Core.csproj", "dotnet/BaseFaq.Common.EntityFramework.Core/"]
COPY ["dotnet/BaseFaq.Common.EntityFramework.Core.Audit/BaseFaq.Common.EntityFramework.Core.Audit.csproj", "dotnet/BaseFaq.Common.EntityFramework.Core.Audit/"]
COPY ["dotnet/BaseFaq.Common.EntityFramework.Core.SoftDelete/BaseFaq.Common.EntityFramework.Core.SoftDelete.csproj", "dotnet/BaseFaq.Common.EntityFramework.Core.SoftDelete/"]
COPY ["dotnet/BaseFaq.Common.EntityFramework.Core.Tenant/BaseFaq.Common.EntityFramework.Core.Tenant.csproj", "dotnet/BaseFaq.Common.EntityFramework.Core.Tenant/"]
COPY ["dotnet/BaseFaq.Models.Common/BaseFaq.Models.Common.csproj", "dotnet/BaseFaq.Models.Common/"]
COPY ["dotnet/BaseFaq.Models.Tenant/BaseFaq.Models.Tenant.csproj", "dotnet/BaseFaq.Models.Tenant/"]
COPY ["dotnet/BaseFaq.Models.User/BaseFaq.Models.User.csproj", "dotnet/BaseFaq.Models.User/"]
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
