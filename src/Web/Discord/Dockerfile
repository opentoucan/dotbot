FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Discord.csproj", "Web/Discord/"]
RUN dotnet restore "Web/Discord/Discord.csproj"
COPY . .
WORKDIR "/src/Web/Discord"
RUN dotnet build "Discord.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Discord.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt update -y && apt upgrade -y
RUN apt install -y ffmpeg
RUN apt-get install -y libopus-dev
RUN apt-get install -y libsodium-dev
ENTRYPOINT ["dotnet", "Discord.dll"]