namespace STD


module Consts =
    let CSVCOLUMNS = [ "name", "uri" ]

    let TEXTDIVIDER = """
==============================================================================
    """

    let README =
        (sprintf """
Simple Traefik Dash (STD)
STD will watch Traefik, and automatically generate a dashboard
STD can also import more routes via CSV.
%s
RULES:
STD generates reachable routes based on two Router predicates:
Host(<xyz>) [required] and PathPrefix(<zyx>) [optional]
CSV imports requires 2 columns:
%s
%s
""" TEXTDIVIDER (CSVCOLUMNS.ToString()) TEXTDIVIDER)

    let ENVPREFIX = "SD"

    let DEFAULTPORTS = [ ("http", 90), ("https", 443) ] |> Map.ofSeq

    let REQTIMEOUT = 5 * 1000

    let POLLINGRATE = 10 * 1000

    let APIROUTERS = "/api/http/routers"

    let WEBSRVPORT = 80
