FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Xkcd.Job/Xkcd.Job.csproj", "Xkcd.Job/Xkcd.Job.csproj"]
COPY ["src/Xkcd.Sdk/Xkcd.Sdk.csproj", "Xkcd.Sdk/Xkcd.Sdk.csproj"]

COPY "src/Xkcd.Job/" "Xkcd.Job"
COPY "src/Xkcd.Sdk/" "Xkcd.Sdk"
RUN dotnet restore "Xkcd.Job/Xkcd.Job.csproj"
RUN dotnet build "Xkcd.Job/Xkcd.Job.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Xkcd.Job/Xkcd.Job.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Xkcd.Job.dll"]
