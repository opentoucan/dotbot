#!/bin/sh

curl -s localhost:4040/api/tunnels | \
  jq .tunnels[].public_url | \
  xargs -I '$' curl -X PATCH \
    -d '{"interactions_endpoint_url": "$/interactions"}' \
    -H "Authorization: Bot ${Discord__Token}" \
    -H "Content-Type: application/json" \
    https://discord.com/api/applications/@me
