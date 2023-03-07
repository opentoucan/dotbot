# Dotbot

A bot to interact with VoIP and instant messaging platforms. At the moment this is just used for Discord, but there is scope in the future to decoupling this.
The core components are written in C# with a MongoDB database for persistence.
Discord interaction uses [Discord.Net](https://www.nuget.org/packages/Discord.Net) to create a websocket connection via the [Discord API](https://discord.com/developers/docs/intro).

# Local Development

Navigate to the /localdev directory, populate the Discord__BotToken environment variable with your Discord bot token, and run docker-compose up.
If you don't have a token already then follow the [instructions](https://discord.com/developers/docs/intro#bots-and-apps) provided by Discord.