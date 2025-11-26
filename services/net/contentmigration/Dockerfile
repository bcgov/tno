
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ENV DOTNET_CLI_HOME=/tmp
ENV PATH="$PATH:/tmp/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to root for package installs
USER 0
# RUN dotnet tool install --global dotnet-ef

WORKDIR /src

COPY ../../../libs/net/packages /root/.nuget/packages
COPY services/net/contentmigration services/net/contentmigration
RUN mv services/net/contentmigration/Docker.NuGet.Config services/net/contentmigration/NuGet.Config
COPY libs/net libs/net

RUN fix_permissions() { while [ $# -gt 0 ] ; do chgrp -R 0 "$1" && chmod -R g=u "$1"; shift; done } && \
    fix_permissions "/tmp"

WORKDIR /src/services/net/contentmigration
RUN dotnet restore
RUN dotnet build -c $ASPNETCORE_ENVIRONMENT -o /build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS deploy

WORKDIR /app
COPY --from=build /build .

# [Optional] Uncomment this section to install additional OS packages.
RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends procps curl libc6-dev libgdiplus

# Run container by default as user with id 1001 (default)
USER 1001

ENTRYPOINT ["dotnet", "TNO.Services.ContentMigration.dll"]
