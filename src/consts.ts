export const TEXT_DIVIDER = `
==============================================================================
`

export const READ_ME = `
Simple Traefik Dash (STD)
STD will watch Traefik, and automatically generate a dashboard
STD can also import more routes via CSV.
${TEXT_DIVIDER}
RULES:

STD generates reachable routes based on two Router predicates:
Host(<xyz>) [required] and PathPrefix(<zyx>) [optional]

CSV imports requires 3 columns:
name, uri, favicon_uri

${TEXT_DIVIDER}
`

export const ENV_PREFIX = "SD"

export const DEFAULT_PORTS = {
  http: 80,
  https: 443,
}

export const REQ_TIMEOUT = 10 * 1000

export const API_ROUTERS = "/api/http/routers"
