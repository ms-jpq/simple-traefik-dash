namespace STD

open System
open System.IO

module Consts =

    let CSVCOLUMNS = [ "name"; "uri" ]

    let CSVDIR = Directory.GetCurrentDirectory() + "/more-routes/"
    let BLOCKLSTDIR = Directory.GetCurrentDirectory() + "/ignore/"

    let TEXTDIVIDER = """
==============================================================================
    """

    let private readme =
        sprintf """
Simple Traefik Dash (STD)
STD will watch Traefik, and automatically generate a dashboard
STD can also import more routes via CSV.
%s
RULES:
STD generates routes based on two Router predicates:
Host(<xyz>) [required] and PathPrefix(<zyx>) [optional]
%s
CSV imports requires 2 columns: [%s]
Place *.csv under: %s
%s
Optionally: Add ignore lists based on Traefik Router names: [name]
Place *.csv under: %s
%s
"""



    let README = readme TEXTDIVIDER "\n" (String.concat "," CSVCOLUMNS) CSVDIR "\n" BLOCKLSTDIR TEXTDIVIDER


    let ENVPREFIX = "SD"

    let DEFAULTPORTS = [ ("http", 90), ("https", 443) ] |> Map.ofSeq

    let REQTIMEOUT = TimeSpan.FromSeconds(5.0)

    let POLLINGRATE = TimeSpan.FromSeconds(10.0)

    let APIROUTERS = "/api/http/routers"

    let WEBSRVPORT = 8080
