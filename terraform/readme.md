# Terraform configuration
This terraform configuration uses [terraform-cloudflare-tunnel-module](https://github.com/Sillock-Inc/terraform-cloudflare-tunnel-module) for provisioning a Cloudflare tunnel instance 

Azure is configured as the remote backend and will use different states for the environments.
Except for the ephemeral pull request environments, these share the same QA state file for simplification

## Setup

```hcl
terraform init -backend-config=backend.conf.example
```

Populate the terraform.tfvars file with any configuration needed

### Notes for QA

This workflow for these ephemeral environments use CLI workspaces for each branch name

## Running the configuration


```hcl
terraform apply -var-file=terraform.tfvars
```
