FROM mcr.microsoft.com/dotnet/sdk:7.0 as build

ENV DOTNET_CLI_HOME=/tmp
ENV PATH="$PATH:/tmp/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to root for package installs
USER 0

WORKDIR /src
COPY services/net/fileupload services/net/fileupload
COPY libs/net libs/net

RUN fix_permissions() { while [ $# -gt 0 ] ; do chgrp -R 0 "$1" && chmod -R g=u "$1"; shift; done } && \
    fix_permissions "/tmp"

WORKDIR /src/services/net/fileupload
RUN dotnet build -c $ASPNETCORE_ENVIRONMENT -o /build

FROM mcr.microsoft.com/dotnet/aspnet:7.0 as deploy

WORKDIR /app
COPY --from=build /build .

USER 1001

ENTRYPOINT ["dotnet", "TNO.Services.FileUpload.dll"]


