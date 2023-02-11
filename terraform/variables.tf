variable "cloudflare_api_token" {
    type        = string
    description = "API token to authenticate resource changes to Cloudflare"
}
variable "cloudflare_account_id" {
    type        = string
    description = "Account ID for the Cloudflare account to provision resources under"
}

variable "tunnel_name" {
    type        = string
    description = "DNS name for the tunnel e.g. `example.com`"
}

variable "ingress_rules" {
    type = list(object({
        subdomain = string
        address   = string
    }))
    description = "Subdomain name and kubernetes KubeDNS name as a key value pair for Cloudflare tunnel config"
}

variable "zone_dns" {
    type        = string
    description = "DNS name for the tunnel e.g. `example.com`"
}