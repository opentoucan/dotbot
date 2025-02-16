ARG SDK_VERSION=9.0
FROM mcr.microsoft.com/devcontainers/dotnet:1-${SDK_VERSION}
ARG SDK_VERSION
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    sudo dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    sudo apt-get update && \
    sudo apt-get install -y dotnet-sdk-$SDK_VERSION
