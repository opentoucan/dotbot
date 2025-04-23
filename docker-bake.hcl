target "docker-metadata-action" {}

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
}

target "migrator" {
  inherits = ["image-release"]
  dockerfile = "./src/Dotbot.Infrastructure/migration.Dockerfile"
}
