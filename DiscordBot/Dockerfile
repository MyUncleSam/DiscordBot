FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /source
COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /app --no-restore

#--------

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS build
WORKDIR /opt/bot
COPY --from=build /app .

VOLUME ["/opt/bot/config"]
ENTRYPOINT ["dotnet", "DiscordBot.dll"]