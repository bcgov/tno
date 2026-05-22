FROM mcr.microsoft.com/dotnet/sdk:9.0

ENV DOTNET_CLI_HOME=/tmp
ENV PATH="$PATH:/tmp/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to root for package installs
USER 0
RUN dotnet tool install --global dotnet-ef --version 9.0.0

WORKDIR /src
COPY . .
RUN fix_permissions() { while [ $# -gt 0 ] ; do chgrp -R 0 "$1" && chmod -R g=u "$1"; shift; done } && \
    fix_permissions "/src" "/tmp"
RUN dotnet restore
RUN dotnet build /src/dal/TNO.DAL.csproj --no-restore -c Release

RUN dotnet ef migrations bundle \
    --project /src/dal/TNO.DAL.csproj \
    --configuration Release \
    --no-build \
    --verbose \
    -o /app/efbundle

RUN chmod +x /app/efbundle
RUN chmod -R 0777 /src
RUN mkdir /.local
RUN chmod -R 0777 /.local

# Run container by default as user with id 1001 (default)
USER 1001

ENTRYPOINT ["/app/efbundle", "--verbose"]
