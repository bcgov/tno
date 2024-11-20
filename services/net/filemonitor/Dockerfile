
FROM mcr.microsoft.com/dotnet/sdk:9.0 as build

ENV DOTNET_CLI_HOME=/tmp
ENV PATH="$PATH:/tmp/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to root for package installs
USER 0
# RUN dotnet tool install --global dotnet-ef

WORKDIR /src

COPY services/net/filemonitor services/net/filemonitor
COPY libs/net libs/net
RUN if [ ! -d services/net/filemonitor/keys ]; then mkdir services/net/filemonitor/keys; fi

RUN fix_permissions() { while [ $# -gt 0 ] ; do chgrp -R 0 "$1" && chmod -R g=u "$1"; shift; done } && \
    fix_permissions "/tmp"

WORKDIR /src/services/net/filemonitor
RUN dotnet build -c $ASPNETCORE_ENVIRONMENT -o /build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 as deploy

# [Optional] Uncomment this section to install additional OS packages.
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends procps curl libc6-dev libgdiplus

WORKDIR /app
COPY --from=build /build .
COPY --from=build /src/services/net/filemonitor/keys/ keys
RUN  chmod -R 0777 keys

# This volume is the local storage for uploaded files.
RUN mkdir /data
VOLUME /data
RUN chmod -R 0777 /data

# Run container by default as user with id 1001 (default)
USER 1001

ENTRYPOINT dotnet TNO.Services.FileMonitor.dll
