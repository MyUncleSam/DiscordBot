FROM mcr.microsoft.com/dotnet/runtime:3.1 AS build

ADD ./files/ /opt/bot

WORKDIR /opt/bot
VOLUME ["/opt/bot/config"]
ENTRYPOINT ["dotnet", "DiscordBot.dll"]