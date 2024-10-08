#!/bin/sh
podman run -it --network host --workdir /data --volume $(pwd):/data quay.io/helmpack/chart-testing:v3.7.1 ct lint /data/charts/dotbot --all --debug --print-config