FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source
COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /opt/bot --no-restore

#--------

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /opt/bot
COPY --from=build /opt/bot .

VOLUME ["/opt/bot/config"]
ENTRYPOINT ["dotnet", "DiscordBot.dll"]