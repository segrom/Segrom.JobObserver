FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5001
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Segrom.JobObserver.OzonService.Host/Segrom.JobObserver.OzonService.Host.csproj", "src/Segrom.JobObserver.OzonService.Host/"]
COPY ["src/Segrom.JobObserver.OzonService.Application/Segrom.JobObserver.OzonService.Application.csproj", "src/Segrom.JobObserver.OzonService.Application/"]
COPY ["src/Segrom.JobObserver.OzonService.Domain/Segrom.JobObserver.OzonService.Domain.csproj", "src/Segrom.JobObserver.OzonService.Domain/"]
COPY ["src/Segrom.JobObserver.OzonService.PostgresRepository/Segrom.JobObserver.OzonService.PostgresRepository.csproj", "src/Segrom.JobObserver.OzonService.PostgresRepository/"]
COPY ["src/Segrom.JobObserver.OzonService.OzonJobApiClient/Segrom.JobObserver.OzonService.OzonJobApiClient.csproj", "src/Segrom.JobObserver.OzonService.OzonJobApiClient/"]
RUN dotnet restore "src/Segrom.JobObserver.OzonService.Host/Segrom.JobObserver.OzonService.Host.csproj"
COPY . .
WORKDIR "/src/src/Segrom.JobObserver.OzonService.Host"
RUN dotnet build "./Segrom.JobObserver.OzonService.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Segrom.JobObserver.OzonService.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Segrom.JobObserver.OzonService.Host.dll"]
