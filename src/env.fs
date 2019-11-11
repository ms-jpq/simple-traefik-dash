namespace STD


open DomainAgnostic
open Microsoft.Extensions.Logging
open System
open Consts

module Env =

    type Variables =
        { logLevel: LogLevel
          traefikAPI: Uri
          entryPoints: string seq
          exitPort: int
          ignoreRoutes: string seq }


    let private prefix = sprintf "%s_%s" ENVPREFIX

    let private required name v = (sprintf "MISSING ENVIRONMENTAL VARIABLE %s" (prefix name), v) ||> Option.ForceUnwrap


    let private pLog find =
        find (prefix "LOG_LEVEL")
        |> Option.bind Parse.Enum<LogLevel>
        |> Option.Recover LogLevel.Warning

    let private pAPI find =
        find (prefix "TRAEFIK_API")
        |> Option.bind Parse.Uri
        |> required "TRAEFIK_API"

    let private pEntryPoints find =
        find (prefix "ENTRY_POINTS")
        |> Option.map (fun (s: string) -> s.Split(","))
        |> Option.bind Seq.NilIfEmpty
        |> required "ENTRY_POINTS"

    let private pExitPort find =
        find (prefix "EXIT_PORT")
        |> Option.bind Parse.Int
        |> required "EXIT_PORT"

    let private pIgnoreRoutes find =
        find (prefix "IGNORE_ROUTES")
        |> Option.map (fun (s: string) -> s.Split(","))
        |> Option.Recover [||]


    let Opts() =
        let find = ENV() |> flip Map.tryFind
        { logLevel = pLog find
          traefikAPI = pAPI find
          entryPoints = pEntryPoints find
          exitPort = pExitPort find
          ignoreRoutes = pIgnoreRoutes find }
