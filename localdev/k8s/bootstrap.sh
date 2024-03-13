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
  echo "Discord Bot token not found, please ensure you have "$BOTTOKEN"" >> /dev/stderr
exit 1
fi

if [[ -z "$BOTPUBKEY" ]]; then 
  echo "Discord Bot public key not found, please ensure you have "$BOTPUBKEY"" >> /dev/stderr
exit 1
fi

if [[ -z "$WEBHOOKURL" ]]; then
  echo "Webhook not found for xkcd channel, please ensure you have "$WEBHOOKURL"" >> /dev/stderr
exit 1
fi

if [[ -z "$NGROK_API_KEY" ]]; then
  echo "ngrok API key is required "$NGROK_API_KEY"" >> /dev/stderr
exit 1
fi

if [[ -z "$NGROK_AUTHTOKEN" ]]; then
  echo "ngrok auth token is required "$NGROK_AUTHTOKEN"" >> /dev/stderr
exit 1
fi

kubectl create secret generic dotbot-secret \
  --from-literal=DiscordSettings__Webhooks__xkcd=$WEBHOOKURL \
  --from-literal=DiscordSettings__BotToken=$BOTTOKEN \
  --from-literal=DiscordSettings__PublicKey=$BOTPUBKEY \
  --from-literal=DiscordSettings__TestGuild=$TESTGUILD \
  --from-literal=MongoDbSettings__DatabaseName="localdev" \
  --from-literal=MongoDbSettings__ConnectionString="mongodb+srv://root:root@mongodb-headless.default.svc.cluster.local?tls=false&ssl=false&replicaSet=rs0" \
  --from-literal=RabbitMQ__Endpoint="rabbitmq.default.svc.cluster.local" \
  --from-literal=RabbitMQ__Username="guest" \
  --from-literal=RabbitMQ__Password="guest" \
  --from-literal=AWS_PROFILE="localdev" \
  --from-literal=S3__Profile="localdev" \
  --from-literal=S3__ServiceURL="http://localstack:4566" \
  --from-literal=S3__ForcePathStyle=true \
  --from-literal=AWS_ACCESS_KEY_ID='localdev' \
  --from-literal=AWS_SECRET_KEY='localdev' 

helm install ngrok-ingress-controller ngrok/kubernetes-ingress-controller \
  --namespace ngrok-ingress-controller \
  --create-namespace \
  --set credentials.apiKey=$NGROK_API_KEY \
  --set credentials.authtoken=$NGROK_AUTHTOKEN

kubectl create ingress dotbot-ingress \
  --class=ngrok \
  --rule=$NGROK_DOMAIN/*=dotbot:80