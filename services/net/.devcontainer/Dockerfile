# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.224.2/containers/dotnet/.devcontainer/base.Dockerfile

# [Choice] .NET version: 7.0, 6.0, 5.0, 3.1, 6.0-bullseye, 5.0-bullseye, 3.1-bullseye, 6.0-focal, 5.0-focal, 3.1-focal
ARG VARIANT="9.0"
FROM mcr.microsoft.com/devcontainers/dotnet:0-${VARIANT}

# [Choice] Node.js version: none, lts/*, 16, 14, 12, 10
ARG NODE_VERSION="none"
RUN if [ "${NODE_VERSION}" != "none" ]; then su vscode -c "umask 0002 && . /usr/local/share/nvm/nvm.sh && nvm install ${NODE_VERSION} 2>&1"; fi

# [Optional] Uncomment this section to install additional OS packages.
RUN apt-get update --fix-missing && apt-get -y upgrade && export DEBIAN_FRONTEND=noninteractive \
    && apt-get -y install --no-install-recommends procps curl ffmpeg \
      libc6-dev libgdiplus libgdiplus libgstreamer1.0-dev libgstreamer-plugins-base1.0-dev libgstreamer-plugins-bad1.0-dev \
      gstreamer1.0-plugins-base gstreamer1.0-plugins-good gstreamer1.0-plugins-bad gstreamer1.0-plugins-ugly \
      gstreamer1.0-libav gstreamer1.0-tools gstreamer1.0-alsa gstreamer1.0-gl gstreamer1.0-gtk3 gstreamer1.0-qt5 gstreamer1.0-pulseaudio
      # gstreamer1.0-x
      # For some reason gstreamer1.0-x cannot be installed.

RUN su vscode -c "dotnet tool install --global dotnet-ef"
# RUN dotnet tool update --global dotnet-ef
ENV PATH="$PATH:/home/vscode/.dotnet/tools"

# [Optional] Uncomment this line to install global node packages.
# RUN su vscode -c "source /usr/local/share/nvm/nvm.sh && npm install -g <your-package-here>" 2>&1
