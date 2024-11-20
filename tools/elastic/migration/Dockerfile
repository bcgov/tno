ARG BUILD_CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

ENV DOTNET_CLI_HOME=/tmp
ENV PATH="$PATH:/tmp/.dotnet/tools"

USER 0

# WORKDIR /src
# RUN fix_permissions() { while [ $# -gt 0 ] ; do chgrp -R 0 "$1" && chmod -R g=u "$1"; shift; done } && \
#     fix_permissions "/src" "/tmp"
# RUN chmod -R 0777 /src
# RUN mkdir /.local
# RUN chmod -R 0777 /.local

COPY tools/elastic/migration/ /src/tools/elastic/migration/
COPY libs/net/ /src/libs/net/

WORKDIR /src/tools/elastic/migration
RUN dotnet restore
RUN dotnet publish "TNO.Elastic.Migration.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

ENV ASPNETCORE_ENVIRONMENT=Production

USER 1001

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TNO.Elastic.Migration.dll"]

# ENTRYPOINT ["tail", "-f", "/dev/null"]

