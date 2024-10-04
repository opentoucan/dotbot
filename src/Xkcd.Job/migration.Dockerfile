ARG DOTNET_RUNTIME=mcr.microsoft.com/dotnet/aspnet:8.0
ARG DOTNET_SDK=mcr.microsoft.com/dotnet/sdk:8.0

FROM ${DOTNET_RUNTIME} AS base
WORKDIR /app
EXPOSE 8080
USER 65534

FROM ${DOTNET_SDK} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["Dotbot.sln", "."]
COPY ["src/Xkcd.Sdk/Xkcd.Sdk.csproj", "Xkcd.Sdk/Xkcd.Sdk.csproj"]
COPY ["src/Xkcd.Job/Xkcd.Job.csproj", "Xkcd.Job/Xkcd.Job.csproj"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/ServiceDefaults.csproj"]

COPY ["src/Xkcd.Sdk/", "Xkcd.Sdk"]
COPY ["src/Xkcd.Job/", "Xkcd.Job"]
COPY ["src/ServiceDefaults/", "ServiceDefaults"]

# Build the migrationbundle here
FROM build as migrationbuilder
ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install --global dotnet-ef
RUN mkdir /migrations
RUN dotnet ef migrations bundle -s /src/Xkcd.Job -p /src/Xkcd.Job -c XkcdContext --self-contained -r linux-x64 -o /migrations/migration

FROM ${DOTNET_RUNTIME} as initcontainer
ENV CONNECTIONSTRING=""
COPY --from=migrationbuilder /migrations /migrations
RUN chmod 755 /migrations/migration
WORKDIR /migrations
ENTRYPOINT ./migration --connection "$CONNECTIONSTRING"
