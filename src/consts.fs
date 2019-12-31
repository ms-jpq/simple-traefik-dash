namespace STD

open System
open System.IO

module Consts =

    let PROJECTURI = "https://ms-jpq.github.io/simple-traefik-dash/"

    let CONTENTROOT = Directory.GetCurrentDirectory()

    let ROUTESDIR = Path.Combine(CONTENTROOT, "more-routes")

    let BLOCKSDIR = Path.Combine(CONTENTROOT, "ignore-routes")

    let RESOURCESDIR = Path.Combine(CONTENTROOT, "views")


    let private readme =
        sprintf """
Simple Traefik Dash (STD)
STD will watch Traefik, and automatically generate a dashboard
STD can also import more routes via CSV.
==============================================================================
RULES:
STD generates routes based on two Router predicates:
Host(<xyz>) [required] and PathPrefix(<zyx>) [optional]
%s
CSV imports requires 2 columns: [name, uri]
Place *.csv under: %s
%s
Optionally: Add ignore lists based on Traefik Router names: [name]
Place *.csv under: %s
==============================================================================
https://ms-jpq.github.io/simple-traefik-dash/
"""


    let README =
        readme "\n" ROUTESDIR "\n" BLOCKSDIR

    let ENVPREFIX = "STD"

    let DEFAULTPORTS = [ ("http", 90), ("https", 443) ] |> Map.ofSeq

    let REQTIMEOUT = TimeSpan.FromSeconds(5.0)

    let POLLINGRATE = TimeSpan.FromSeconds(10.0)

    let APIROUTERS = "/api/http/routers"

    let WEBSRVPORT = 5050

    let DEFAULTTITLE = "Simple Traefik Dash"
