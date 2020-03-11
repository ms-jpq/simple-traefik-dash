# [Simple Traefik Dash](https://ms-jpq.github.io/simple-traefik-dash/)

[![Docker Pulls](https://img.shields.io/docker/pulls/msjpq/simple-traefik-dash.svg)](https://hub.docker.com/r/msjpq/simple-traefik-dash/)

**Zero conf** service dashboard for [Traefik v2](https://traefik.io/) reverse proxy

Parses Traefik's [Routers](https://docs.traefik.io/routing/overview/) and generates all deterministic routes

`Tested for Docker, Docker Swarm, and Kubernetes!`

## Screenshot

**You can totally change this background, don't worry**

![promo img](https://raw.githubusercontent.com/ms-jpq/simple-traefik-dash/master/example/screenshot.png)

## Usage

```yml
# SIMPLE TRAEFIK DASH #
simple-traefik-dash:
  image: msjpq/simple-traefik-dash
  labels:
    - traefik.http.services.std.loadbalancer.server.port=5050
    - traefik.http.routers.std.tls.options=default
    - traefik.http.routers.std.rule=Host("<Something>")
  environment:
    - STD_TRAEFIK_API=http://traefik:8080/ # Required, make sure you can actually talk to Traefik
    - STD_TRAEFIK_ENTRY_POINTS=web-secure # Required, only routes using entrypoints will be parsed
    - STD_TRAEFIK_EXIT_PORT=443 # Required, your exit port
    - STD_KUBECRD_FIX=True # Optional, enable if you see <ns>-name-<randomstring> with in Kubernetes

    - STD_TITLE=🐳 # Optional, page title
    - STD_BACKGROUND=https://... # Optional, changes the background image
    - STD_PATH_PREFIX=/std # Optional, for reverse proxy
  volumes:
    - ./more.csv:/std/more-routes/more.csv # Optional - CSV Columns: name, uri
    - ./ignore.csv:/std/ignore-routes/ignore.csv # Optional - CSV Column: name
```

You can test your Routers rules like so:

`docker run --rm msjpq/simple-traefik-dash "<RULE>"`

i.e.

`docker run --rm msjpq/simple-traefik-dash  "Host('qbz', 'kfc') && (PathPrefix('chicken') || PathPrefix( '95' )) || Host('dog.org', 'otters.moo') && PathPrefix('cat')"`

## Customization

You can customize all you want! `js`, `css`, `images`, everything!

Simply override the files under `/std/views`, all the static content will be served

## Sister

Check out my sister: [Simple Traefik Identity](https://ms-jpq.github.io/simple-traefik-identity/)

Simple & Configurable -- SSO, for Traefik.
