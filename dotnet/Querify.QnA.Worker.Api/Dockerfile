FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5030

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet/Querify.QnA.Worker.Api/Querify.QnA.Worker.Api.csproj", "dotnet/Querify.QnA.Worker.Api/"]
COPY ["dotnet/Querify.QnA.Worker.Business.Source/Querify.QnA.Worker.Business.Source.csproj", "dotnet/Querify.QnA.Worker.Business.Source/"]
COPY ["dotnet/Querify.Common.EntityFramework.Tenant/Querify.Common.EntityFramework.Tenant.csproj", "dotnet/Querify.Common.EntityFramework.Tenant/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core/Querify.Common.EntityFramework.Core.csproj", "dotnet/Querify.Common.EntityFramework.Core/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core.Audit/Querify.Common.EntityFramework.Core.Audit.csproj", "dotnet/Querify.Common.EntityFramework.Core.Audit/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core.AutoHistory/Querify.Common.EntityFramework.Core.AutoHistory.csproj", "dotnet/Querify.Common.EntityFramework.Core.AutoHistory/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core.SoftDelete/Querify.Common.EntityFramework.Core.SoftDelete.csproj", "dotnet/Querify.Common.EntityFramework.Core.SoftDelete/"]
COPY ["dotnet/Querify.Common.EntityFramework.Core.Tenant/Querify.Common.EntityFramework.Core.Tenant.csproj", "dotnet/Querify.Common.EntityFramework.Core.Tenant/"]
COPY ["dotnet/Querify.Common.Infrastructure.ApiErrorHandling/Querify.Common.Infrastructure.ApiErrorHandling.csproj", "dotnet/Querify.Common.Infrastructure.ApiErrorHandling/"]
COPY ["dotnet/Querify.Common.Infrastructure.Core/Querify.Common.Infrastructure.Core.csproj", "dotnet/Querify.Common.Infrastructure.Core/"]
COPY ["dotnet/Querify.Common.Infrastructure.Telemetry/Querify.Common.Infrastructure.Telemetry.csproj", "dotnet/Querify.Common.Infrastructure.Telemetry/"]
COPY ["dotnet/Querify.Common.Infrastructure.MediatR/Querify.Common.Infrastructure.MediatR.csproj", "dotnet/Querify.Common.Infrastructure.MediatR/"]
COPY ["dotnet/Querify.Common.Infrastructure.Hangfire/Querify.Common.Infrastructure.Hangfire.csproj", "dotnet/Querify.Common.Infrastructure.Hangfire/"]
COPY ["dotnet/Querify.Common.Infrastructure.Storage/Querify.Common.Infrastructure.Storage.csproj", "dotnet/Querify.Common.Infrastructure.Storage/"]
COPY ["dotnet/Querify.Models.Common/Querify.Models.Common.csproj", "dotnet/Querify.Models.Common/"]
COPY ["dotnet/Querify.Models.QnA/Querify.Models.QnA.csproj", "dotnet/Querify.Models.QnA/"]
COPY ["dotnet/Querify.Models.Tenant/Querify.Models.Tenant.csproj", "dotnet/Querify.Models.Tenant/"]
COPY ["dotnet/Querify.Models.User/Querify.Models.User.csproj", "dotnet/Querify.Models.User/"]
COPY ["dotnet/Querify.QnA.Common.Domain/Querify.QnA.Common.Domain.csproj", "dotnet/Querify.QnA.Common.Domain/"]
COPY ["dotnet/Querify.QnA.Common.Persistence.HangfireQnaDb/Querify.QnA.Common.Persistence.HangfireQnaDb.csproj", "dotnet/Querify.QnA.Common.Persistence.HangfireQnaDb/"]
COPY ["dotnet/Querify.QnA.Common.Persistence.QnADb/Querify.QnA.Common.Persistence.QnADb.csproj", "dotnet/Querify.QnA.Common.Persistence.QnADb/"]
RUN dotnet restore "dotnet/Querify.QnA.Worker.Api/Querify.QnA.Worker.Api.csproj"
COPY . .
WORKDIR "/src/dotnet/Querify.QnA.Worker.Api"
RUN dotnet build "Querify.QnA.Worker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Querify.QnA.Worker.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Querify.QnA.Worker.Api.dll"]
