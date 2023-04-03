FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Dotbot/Dotbot.csproj", "Dotbot/Dotbot.csproj"]
COPY ["Dotbot.Common/Dotbot.Common.csproj", "Dotbot.Common/Dotbot.Common.csproj"]
COPY ["Dotbot.Database/Dotbot.Database.csproj", "Dotbot.Database/Dotbot.Database.csproj"]
COPY ["Dotbot.Discord/Dotbot.Discord.csproj", "Dotbot.Discord/Dotbot.Discord.csproj"]
RUN dotnet restore "Dotbot/Dotbot.csproj"
COPY . .
WORKDIR "/src/Dotbot"
RUN dotnet build "Dotbot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dotbot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt update -y && apt upgrade -y
RUN apt install -y ffmpeg
RUN apt-get install -y libopus-dev
RUN apt-get install -y libsodium-dev
ENTRYPOINT ["dotnet", "Dotbot.dll"]
