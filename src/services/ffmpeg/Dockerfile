
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ENV DOTNET_CLI_HOME=/tmp
ENV PATH="$PATH:/tmp/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to root for package installs
USER 0

WORKDIR /src
COPY src/services/ffmpeg services/ffmpeg
COPY src/libs libs

RUN fix_permissions() { while [ $# -gt 0 ] ; do chgrp -R 0 "$1" && chmod -R g=u "$1"; shift; done } && \
    fix_permissions "/tmp"

WORKDIR /src/services/ffmpeg
RUN dotnet build -c $ASPNETCORE_ENVIRONMENT -o /build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS deploy

RUN apt-get update --fix-missing && apt-get -y upgrade && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends procps curl ffmpeg libc6-dev libgdiplus

WORKDIR /app
COPY --from=build /build .

# This volume is the local storage for uploaded files.
RUN mkdir /data
VOLUME /data

# Run container by default as user with id 1001 (default)
USER 1001

ENTRYPOINT ["dotnet", "TNO.Services.FFmpeg.dll"]
