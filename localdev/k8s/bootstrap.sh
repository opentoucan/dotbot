#!/bin/sh

if ! command -v kind >> /dev/stderr
then
  echo "Kind could not be found, ensure you have installed it"
  exit 1
fi

if ! command -v kubectl >> /dev/stderr
then
  echo "Kubectl could not be found, ensure you have installed it"
  exit 1
fi


kind create cluster
kubectl config use-context kind-kind

if [[ -z "$BOTTOKEN" ]]; then 
  echo "Discord Bot token not found, please ensure you have $BOTTOKEN" >> /dev/stderr
exit 1
fi

if [[ -z "$BOTPUBKEY" ]]; then 
  echo "Discord Bot public key not found, please ensure you have $BOTPUBKEY" >> /dev/stderr
exit 1
fi

kubectl create secret generic dotbot-secret \
  --from-literal=DiscordSettings__BotToken=$BOTTOKEN \
  --from-literal=DiscordSettings__PublicKey=$BOTPUBKEY \
  --from-literal=MongoDbSettings__DatabaseName="localdev" \
  --from-literal=MongoDbSettings__ConnectionString="mongodb://root:root@mongo:27017/?authMechanism=DEFAULT" \
  --from-literal=RabbitMQ__Endpoint="rabbitmq" \
  --from-literal=RabbitMQ__Username="guest" \
  --from-literal=RabbitMQ__Password="guest" \
  --from-literal=AWS_PROFILE="localdev" \
  --from-literal=localdev__ServiceURL="http://localstack:4566"