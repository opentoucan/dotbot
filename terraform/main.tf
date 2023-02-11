module "cloudflare_tunnel" {
  source = "github.com/Sillock-Inc/terraform-cloudflare-tunnel-module?ref=v0.1.1"

  cloudflare_account_id = var.cloudflare_account_id
  tunnel_name           = var.tunnel_name
  ingress_rules         = var.ingress_rules
  zone_dns              = var.zone_dns
}