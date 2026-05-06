FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5010
EXPOSE 5011

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["dotnet/Querify.QnA.Portal.Api/Querify.QnA.Portal.Api.csproj", "dotnet/Querify.QnA.Portal.Api/"]
RUN dotnet restore "dotnet/Querify.QnA.Portal.Api/Querify.QnA.Portal.Api.csproj"
COPY . .
WORKDIR "/src/dotnet/Querify.QnA.Portal.Api"
RUN dotnet build "./Querify.QnA.Portal.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Querify.QnA.Portal.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Querify.QnA.Portal.Api.dll"]
