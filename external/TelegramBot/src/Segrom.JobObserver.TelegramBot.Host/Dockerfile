FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["external/TelegramBot/Segrom.JobObserver.TelegramBot.Host/Segrom.JobObserver.TelegramBot.Host.csproj", "external/TelegramBot/Segrom.JobObserver.TelegramBot.Host/"]
COPY ["external/TelegramBot/Segrom.JobObserver.TelegramBot.KafkaConsumer/Segrom.JobObserver.TelegramBot.KafkaConsumer.csproj", "external/TelegramBot/Segrom.JobObserver.TelegramBot.KafkaConsumer/"]
COPY ["external/TelegramBot/Segrom.JobObserver.TelegramBot.Client/Segrom.JobObserver.TelegramBot.Client.csproj", "external/TelegramBot/Segrom.JobObserver.TelegramBot.Client/"]
COPY ["external/TelegramBot/Segrom.JobObserver.TelegramBot.Application/Segrom.JobObserver.TelegramBot.Application.csproj", "external/TelegramBot/Segrom.JobObserver.TelegramBot.Application/"]
COPY ["external/TelegramBot/Segrom.JobObserver.TelegramBot.PostgresRepository/Segrom.JobObserver.TelegramBot.PostgresRepository.csproj", "external/TelegramBot/Segrom.JobObserver.TelegramBot.PostgresRepository/"]
RUN dotnet restore "external/TelegramBot/Segrom.JobObserver.TelegramBot.Host/Segrom.JobObserver.TelegramBot.Host.csproj"
COPY . .
WORKDIR "/src/external/TelegramBot/Segrom.JobObserver.TelegramBot.Host"
RUN dotnet build "Segrom.JobObserver.TelegramBot.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Segrom.JobObserver.TelegramBot.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Segrom.JobObserver.TelegramBot.Host.dll"]
