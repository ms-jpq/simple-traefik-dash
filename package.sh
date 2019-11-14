#!/bin/bash

set -eu
set -o pipefail

# docker-compose up -d --build

docker tag $1 docker.pkg.github.com/ms-jpq/simple-traefik-dash/simple-traefik-dash:latest

docker push docker.pkg.github.com/ms-jpq/simple-traefik-dash/simple-traefik-dash:latest
