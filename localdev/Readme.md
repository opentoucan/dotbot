# Local development

## Prerequisites
If you want to run these solution locally then install the following tools based on the method of choice below
I recommend you create an .envrc file in the root of this project and populate it with the following environment variables
```
export NGROK_AUTHTOKEN=<Your ngrok auth token>
export NGROK_API_KEY=<Your ngrok API key>
export NGROK_DOMAIN=<Your registered ngrok domain (if using Kubernetes below)>
export WEBHOOKURL=<Discord webhook URL for testing>
export BOTPUBKEY=<Your Discord application public key>
export BOTTOKEN=<Your Discord bot private token>

```
### Docker Compose

- Docker
- An ngrok account with an API token and auth token

Navigate to localdev/docker and run `docker-compose up -d`.
To tear down run `docker-compose down`

### Kubernetes

- Docker
- An ngrok account with an API token and auth token
- Kubectl CLI
- Helm CLI
- K8s Kind CLI
- Tilt

Navigate to the root of the repository and run `./localdev/k8s/bootstrap.sh`.
Then run `tilt up` to bring up the application services within the cluster
When you're finished you can run `./localdev/k8s/teardown.sh` to remove the cluster