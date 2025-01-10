ARG DOTNET_RUNTIME=mcr.microsoft.com/dotnet/aspnet:9.0
ARG DOTNET_SDK=mcr.microsoft.com/dotnet/sdk:9.0

FROM ${DOTNET_RUNTIME} AS base
WORKDIR /app
EXPOSE 8080
USER 2048

FROM ${DOTNET_SDK} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Dotbot.Gateway/Dotbot.Gateway.csproj", "Dotbot.Gateway/Dotbot.Gateway.csproj"]
COPY ["src/Dotbot.Infrastructure/Dotbot.Infrastructure.csproj", "Dotbot.Infrastructure/Dotbot.Infrastructure.csproj"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/ServiceDefaults.csproj"]

COPY ["src/Dotbot.Gateway/", "Dotbot.Gateway"]
COPY ["src/Dotbot.Infrastructure/", "Dotbot.Infrastructure"]
COPY ["src/ServiceDefaults/", "ServiceDefaults"]
RUN dotnet restore "Dotbot.Gateway/Dotbot.Gateway.csproj"
RUN dotnet build "Dotbot.Gateway/Dotbot.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Dotbot.Gateway/Dotbot.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dotbot.Gateway.dll"]
