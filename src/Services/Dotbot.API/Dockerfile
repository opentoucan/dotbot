FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

#WORKDIR /src/Services/Dotbot.API

COPY "Dotbot.sln" "Dotbot.sln"
COPY "Services/Dotbot.API/Dotbot.API.csproj" "Services/Dotbot.API/Dotbot.API.csproj"
COPY "Services/Meme/Meme.API/Meme.API.csproj" "Services/Meme/Meme.API/Meme.API.csproj"
COPY "Services/Meme/Meme.Domain/Meme.Domain.csproj" "Services/Meme/Meme.Domain/Meme.Domain.csproj"
COPY "Services/Meme/Meme.Infrastructure/Meme.Infrastructure.csproj" "Services/Meme/Meme.Infrastructure/Meme.Infrastructure.csproj"
COPY "Services/Xkcd/Xkcd.API/Xkcd.API.csproj" "Services/Xkcd/Xkcd.API/Xkcd.API.csproj"
COPY "Web/Discord/Discord.csproj" "Web/Discord/Discord.csproj"
RUN dotnet restore "Dotbot.sln"

COPY . .
WORKDIR /src/Services/Dotbot.API
RUN dotnet publish --no-restore -c Release -o /app/out

FROM base AS final
WORKDIR /app/out
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "Dotbot.API.dll"]
