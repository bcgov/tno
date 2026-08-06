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

RUN ConnectionStrings__TNO="Host=placeholder;Database=placeholder" \
    DB_POSTGRES_USERNAME=placeholder \
    DB_POSTGRES_PASSWORD=placeholder \
    dotnet ef migrations bundle \
    --project /src/dal/TNO.DAL.csproj \
    --configuration Release \
    --no-build \
    --verbose \
    -o /app/efbundle

# SeedMigration loads each migration's Pre/Post seed scripts from
# <bundle directory>/Migrations at runtime, and silently skips them when the
# folder is absent. The bundle is a single file, so copy the scripts beside it.
RUN cp -r /src/dal/bin/Release/net*/Migrations /app/Migrations && \
    test "$(find /app/Migrations -name '*.sql' | wc -l)" -gt 0

RUN chmod +x /app/efbundle
RUN chgrp -R 0 /app && chmod -R g=u /app
RUN chmod -R 0777 /src
RUN mkdir /.local
RUN chmod -R 0777 /.local

# Run container by default as user with id 1001 (default)
USER 1001

ENTRYPOINT ["/app/efbundle", "--verbose"]
