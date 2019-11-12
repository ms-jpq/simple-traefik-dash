namespace STD


open DomainAgnostic
open Microsoft.Extensions.Logging
open System
open Consts

module Env =

    type Variables =
        { logLevel: LogLevel
          apiPort: int
          traefikAPI: Uri
          entryPoints: string Set
          exitPort: int
          ignoreRoutes: string Set }


    let private prefix = sprintf "%s_%s" ENVPREFIX

    let private required name =
        let err =
            sprintf "\n\n\n-- MISSING ENVIRONMENTAL VARIABLE :: [%s] --\n\n\n" (prefix name)
        err |> Option.ForceUnwrap


    let private pLog find =
        find (prefix "LOG_LEVEL")
        |> Option.bind Parse.Enum<LogLevel>
        |> Option.Recover LogLevel.Warning

    let private pPort find =
        find (prefix "PORT")
        |> Option.bind Parse.Int
        |> Option.Recover WEBSRVPORT

    let private pAPI find =
        find (prefix "TRAEFIK_API")
        |> Option.bind ((fun (str: string) -> str + APIROUTERS) >> Parse.Uri)
        |> required "TRAEFIK_API"

    let private pEntryPoints find =
        find (prefix "TRAEFIK_ENTRY_POINTS")
        |> Option.map (fun (s: string) -> s.Split(","))
        |> Option.bind Seq.NilIfEmpty
        |> Option.map Set
        |> required "TRAEFIK_ENTRY_POINTS"

    let private pExitPort find =
        find (prefix "TRAEFIK_EXIT_PORT")
        |> Option.bind Parse.Int
        |> required "TRAEFIK_EXIT_PORT"

    let private pIgnoreRoutes find =
        find (prefix "TRAEFIK_IGNORE_ROUTES")
        |> Option.map (fun (s: string) -> s.Split(","))
        |> Option.defaultValue [||]
        |> Set

    let Opts() =
        let find = ENV() |> flip Map.tryFind
        { logLevel = pLog find
          apiPort = pPort find
          traefikAPI = pAPI find
          entryPoints = pEntryPoints find
          exitPort = pExitPort find
          ignoreRoutes = pIgnoreRoutes find }
