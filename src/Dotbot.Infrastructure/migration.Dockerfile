# renovate: datasource=dotnet-version depName=dotnet-runtime
ARG DOTNET_RUNTIME=mcr.microsoft.com/dotnet/aspnet:9.0.2-bookworm-slim
# renovate: datasource=dotnet-version depName=dotnet-sdk
ARG DOTNET_SDK=mcr.microsoft.com/dotnet/sdk:9.0.200

FROM ${DOTNET_RUNTIME} AS base
WORKDIR /app
EXPOSE 8080
USER 1024

FROM ${DOTNET_SDK} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Dotbot.sln", "."]
COPY ["src/Dotbot.Api/Dotbot.Api.csproj", "Dotbot.Api/Dotbot.Api.csproj"]
COPY ["src/Dotbot.Infrastructure/Dotbot.Infrastructure.csproj", "Dotbot.Infrastructure/Dotbot.Infrastructure.csproj"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/ServiceDefaults.csproj"]

COPY ["src/Dotbot.Api/", "Dotbot.Api"]
COPY ["src/Dotbot.Infrastructure/", "Dotbot.Infrastructure"]
COPY ["src/ServiceDefaults/", "ServiceDefaults"]

# Build the migrationbundle here
FROM build as migrationbuilder
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install --global dotnet-ef
RUN mkdir /migrations
RUN dotnet ef migrations bundle -s /src/Dotbot.Api -p /src/Dotbot.Infrastructure -c DotbotContext --self-contained -r linux-x64 -o /migrations/migration

FROM ${DOTNET_RUNTIME} as initcontainer
ENV CONNECTIONSTRING=""
COPY --from=migrationbuilder /migrations /migrations
RUN chmod 755 /migrations/migration
WORKDIR /migrations
ENTRYPOINT ./migration --connection "$CONNECTIONSTRING"
