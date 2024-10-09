FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Dotbot.Gateway/Dotbot.Gateway.csproj", "Dotbot.Gateway/Dotbot.Gateway.csproj"]
COPY ["src/Dotbot.Infrastructure/Dotbot.Infrastructure.csproj", "Dotbot.Infrastructure/Dotbot.Infrastructure.csproj"]
COPY ["src/Xkcd.Sdk/Xkcd.Sdk.csproj", "Xkcd.Sdk/Xkcd.Sdk.csproj"]
COPY ["src/ServiceDefaults/ServiceDefaults.csproj", "ServiceDefaults/ServiceDefaults.csproj"]

COPY ["src/Dotbot.Gateway/", "Dotbot.Gateway"]
COPY ["src/Dotbot.Infrastructure/", "Dotbot.Infrastructure"]
COPY ["src/Xkcd.Sdk/", "Xkcd.Sdk"]
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
