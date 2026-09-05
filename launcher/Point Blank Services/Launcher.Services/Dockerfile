FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PointBlank.Socket.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish PointBlank.Socket.csproj -c Release -o /out \
    -p:OutputType=Exe \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out/ ./
ENV FL_EVIDENCE_ROOT=/app/Evidence
EXPOSE 9000/tcp
ENTRYPOINT ["dotnet", "Socket.dll"]
