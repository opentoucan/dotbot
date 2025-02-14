# Dotbot

An opinionated scalable Discord bot. Instead of using a long lived websocket connection this application relies solely on [Discord's Interaction framework](https://discord.com/developers/docs/interactions/overview).

# Instructions

[![Open in Dev Containers](https://img.shields.io/static/v1?label=Dev%20Containers&message=Open&color=blue)](https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/opentoucan/dotbot)

This project contains a dev container with all necessary pre-requisites for running the application yourself.
Install the dev containers extension in VS Code or Jetbrains Rider, or download the [CLI tool](https://github.com/devcontainers/cli) if you prefer to use your own IDE

Before you can authenticate you'll need to create your own devcontainer.env file within the .devcontainer folder, checkout the example.env file for all the variables you'll need.

> [!NOTE]
> You'll need to create an ngrok account and [auth token](https://dashboard.ngrok.com/get-started/your-authtoken) to route Discord calls to your local app instance via an ngrok container. This saves you from opening your firewall and setting up routing rules.
