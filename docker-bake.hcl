target "docker-metadata-action" {
  tags = []
}

variable "TAG_BASE" {}

target "image" {
  inherits = ["docker-metadata-action"]
}

target "image-local" {
  inherits = ["image"]
  output = ["type=docker"]
}

target "image-release" {
  inherts = ["image"]
  platforms = [
    "linux/amd64",
    "linux/arm64"
  ]
}

target "image-all" {
  name = "dotbot-${tgt}"
  matrix = {
    tgt = ["api", "migrator"]
  }
  target = tgt
}

target "api" {
  inherits = ["image-release"]
  dockerfile = "./src/Dotbot.Api/Dockerfile"
  tags = [for tag in target.docker-metadata-action.tags : "${TAG_BASE}/dotbot.api:${tag}"]
}

target "migrator" {
  inherits = ["image-release"]
  dockerfile = "./src/Dotbot.Infrastructure/migration.Dockerfile"
  tags = [for tag in target.docker-metadata-action.tags : "${TAG_BASE}/dotbot.migrator:${tag}"]
}
