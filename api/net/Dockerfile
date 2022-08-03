ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
EXPOSE 443 8080

# Copy csproj and restore as distinct layers
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src/api/net
COPY api/net/ .
COPY libs/net/ /src/libs/net/

RUN dotnet restore
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef --version 6.0.3

RUN dotnet build "TNO.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
RUN dotnet publish "TNO.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish

# Runtime image
FROM base AS final

RUN apt-get update && apt-get -y upgrade
RUN apt -y install curl libc6-dev

WORKDIR /app
COPY --from=publish /app/publish .
COPY api/net/entrypoint.sh .
RUN chmod +x /app/entrypoint.sh

RUN useradd --create-home appuser

# This volume is the local storage for uploaded files.
RUN mkdir /data
VOLUME /data
RUN chmod -R 0777 /data

# This volume is the local storage for capture A/V files.
RUN mkdir /av
VOLUME /av
RUN chmod -R 0777 /av

USER appuser
ENTRYPOINT ["/app/entrypoint.sh"]

# ENTRYPOINT ["tail", "-f", "/dev/null"]
