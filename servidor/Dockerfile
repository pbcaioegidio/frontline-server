FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Plugin.Core/Plugin.Core.csproj Plugin.Core/
COPY Server.Auth/Server.Auth.csproj Server.Auth/
COPY Server.Game/Server.Game.csproj Server.Game/
COPY Server.Match/Server.Match.csproj Server.Match/
COPY Server.Host/Server.Host.csproj Server.Host/
RUN dotnet restore Server.Host/Server.Host.csproj
COPY Plugin.Core/ Plugin.Core/
COPY Server.Auth/ Server.Auth/
COPY Server.Game/ Server.Game/
COPY Server.Match/ Server.Match/
COPY Server.Host/ Server.Host/
RUN dotnet publish Server.Host/Server.Host.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out/ ./
ENV PB_CONTENT_ROOT=/app
EXPOSE 30000/tcp 39190/tcp 39191/tcp 39192/tcp 39193/tcp 40009/udp
ENTRYPOINT ["dotnet", "FLMonitor.dll"]
