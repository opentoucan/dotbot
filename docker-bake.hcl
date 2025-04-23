target "docker-metadata-action" {}

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
  name = "dotbot.${tgt}"
  matrix = {
    tgt = ["api", "migrator"]
  }
  target = tgt
}

target "api" {
  inherits = ["image-release"]
  name = "dotbot.api"
  context = "./src/Dotbot.Api"
  dockerfile = "Dockerfile"
}

target "migrator" {
  inherits = ["image-release"]
  name = "dotbot.migrator"
  context = "./src/Dotbot.Infrastructure"
  dockerfile = "migration.Dockerfile"
}
