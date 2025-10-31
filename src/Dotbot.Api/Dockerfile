# renovate: datasource=dotnet-version depName=dotnet-runtime
ARG DOTNET_RUNTIME=mcr.microsoft.com/dotnet/aspnet:9.0.10-bookworm-slim
# renovate: datasource=dotnet-version depName=dotnet-sdk
ARG DOTNET_SDK=mcr.microsoft.com/dotnet/sdk:9.0.306

FROM ${DOTNET_RUNTIME} AS base
WORKDIR /app
EXPOSE 8080
USER 2048

FROM ${DOTNET_SDK} AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_VERSION=1.0.0
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Dotbot.Api/Dotbot.Api.csproj", "Dotbot.Api/Dotbot.Api.csproj"]
COPY ["src/Dotbot.Infrastructure/Dotbot.Infrastructure.csproj", "Dotbot.Infrastructure/Dotbot.Infrastructure.csproj"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/ServiceDefaults.csproj"]

COPY ["src/Dotbot.Api/", "Dotbot.Api"]
COPY ["src/Dotbot.Infrastructure/", "Dotbot.Infrastructure"]
COPY ["src/ServiceDefaults/", "ServiceDefaults"]

RUN dotnet restore "Dotbot.Api/Dotbot.Api.csproj"
RUN dotnet build "Dotbot.Api/Dotbot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_VERSION=1.0.0
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Dotbot.Api/Dotbot.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Version=${BUILD_VERSION}

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
ENV LANG=en_GB.UTF-8
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dotbot.Api.dll"]
