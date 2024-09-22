# --- build dotnet solution
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c release -o /opt/bot --no-restore

# --- build image from build output

FROM mcr.microsoft.com/dotnet/runtime:8.0

ARG TARGETPLATFORM
ARG BUILDPLATFORM

WORKDIR /opt/bot
COPY --from=build /opt/bot .

LABEL TARGETPLATFORM=${TARGETPLATFORM}
LABEL BUILDPLATFORM=${BUILDPLATFORM}

ENV TZ="Europe/Berlin"

VOLUME ["/opt/bot/config"]
ENTRYPOINT ["dotnet", "DiscordBot.dll"]