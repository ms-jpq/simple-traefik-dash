#!/bin/bash

set -eu
set -o pipefail

# docker-compose up -d --build

docker tag $1 msjpq/simple-traefik-dash:latest

docker push msjpq/simple-traefik-dash:latest
