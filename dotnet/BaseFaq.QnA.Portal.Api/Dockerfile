FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5030
EXPOSE 5031

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet/BaseFaq.QnA.Portal.Api/BaseFaq.QnA.Portal.Api.csproj", "dotnet/BaseFaq.QnA.Portal.Api/"]
RUN dotnet restore "dotnet/BaseFaq.QnA.Portal.Api/BaseFaq.QnA.Portal.Api.csproj"
COPY . .
WORKDIR "/src/dotnet/BaseFaq.QnA.Portal.Api"
RUN dotnet build "./BaseFaq.QnA.Portal.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BaseFaq.QnA.Portal.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BaseFaq.QnA.Portal.Api.dll"]
