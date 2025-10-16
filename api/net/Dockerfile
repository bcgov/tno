ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# RUN apt-get update --fix-missing && apt-get -y upgrade
# RUN apt -y install curl libc6-dev libgdiplus ffmpeg
# RUN apt-get clean

EXPOSE 443 8080

# Copy csproj and restore as distinct layers
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# ENV PATH="$PATH:/root/.dotnet/tools"
# RUN dotnet tool install --global dotnet-ef --version 7.0.13

WORKDIR /src/api/net
COPY api/net/ .
COPY libs/net/ /src/libs/net/

RUN dotnet restore
RUN dotnet publish "TNO.API.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish

# Runtime image
FROM base AS final

WORKDIR /app
COPY --from=build /app/publish .
COPY api/net/entrypoint.sh .
RUN chmod +x /app/entrypoint.sh

# RUN useradd --create-home appuser

# This volume is the local storage for uploaded files.
RUN mkdir /data
VOLUME /data
RUN chgrp -R 0 /data && \
    chmod -R g=u /data && \
    chown -R 1001 /data

# This volume is the local storage for capture A/V files.
RUN mkdir /av
VOLUME /av
RUN chgrp -R 0 /av && \
    chmod -R g=u /av && \
    chown -R 1001 /av

# USER appuser
USER 1001
ENTRYPOINT ["/app/entrypoint.sh"]

# ENTRYPOINT ["tail", "-f", "/dev/null"]
