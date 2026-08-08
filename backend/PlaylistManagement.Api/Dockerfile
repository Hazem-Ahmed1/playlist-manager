# Build context is expected to be the `backend/` folder (see docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PlaylistManagement.Api/PlaylistManagement.Api.csproj PlaylistManagement.Api/
RUN dotnet restore PlaylistManagement.Api/PlaylistManagement.Api.csproj

COPY PlaylistManagement.Api/ PlaylistManagement.Api/
RUN dotnet publish PlaylistManagement.Api/PlaylistManagement.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app .

# Seeded demo audio/cover files ship inside the image so a brand-new named
# volume gets pre-populated with them on first run (Docker copies the
# image's directory contents into an empty volume the first time it mounts).
VOLUME ["/app/wwwroot/uploads"]

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 8080

ENTRYPOINT ["dotnet", "PlaylistManagement.Api.dll"]
