namespace STD.Services

open DomainAgnostic
open DomainAgnostic.Timers
open STD.Env
open STD.Consts
open STD.State
open STD.Parsers.Traefik
open STD.Parsers.CSV
open System
open Microsoft.Extensions.Logging
open DomainAgnostic.Globals
open Microsoft.Extensions.Hosting
open System.Net.Http
open System.Security.Authentication


type PollingService(logger: ILogger<PollingService>, deps: Container<Variables>, state: GlobalVar<State>) =
    inherit BackgroundService()

    let client =
        let handler = new HttpClientHandler()
        handler.SslProtocols <-
            SslProtocols.None ||| SslProtocols.Tls ||| SslProtocols.Tls11 ||| SslProtocols.Tls12 ||| SslProtocols.Tls13
        let c = new HttpClient(handler)
        c.Timeout <- REQTIMEOUT
        c

    let wait = NewTicker POLLINGRATE

    let parseOpts =
        { exitPort = deps.Boxed.exitPort
          entryPoints = deps.Boxed.entryPoints
          fixKubeCRD = deps.Boxed.fixKubCRD }

    let parse = materialize parseOpts

    let poll _ =
        async {
            let! _csv = pCSV() |> Async.StartChild
            let! _traefik = client.GetStringAsync(deps.Boxed.traefikAPI)
                            |> Async.AwaitTask
                            |> Async.StartChild
            let! traefik = _traefik
            let! (r1, ignore, errs) = _csv
            let (r2, failed) = parse traefik |> Result.ForceUnwrap
            let ignoring = Set ignore

            let routes =
                r2
                |> Seq.filter (fun r -> (Set.contains r.name >> not) ignoring)
                |> (++) r1
                |> Seq.sortBy (fun r -> r.name)

            let ns =
                { lastupdate = Some DateTime.UtcNow
                  errors = errs
                  ignoring = ignoring
                  routes =
                      { succ = routes
                        fail = failed } }

            errs |> Seq.iter logger.LogError

            return! ns |> state.Put
        }

    let mutable agent = Option<Agent<unit>>.None

    let errHandle (err: exn) prev =
        async {
            logger.LogError(err.Message, err.StackTrace)
            do! wait() |> Async.Ignore
            return prev
        }

    let runloop _ _ =
        async {
            do! poll()
            do! wait() |> Async.Ignore
        }

    override __.ExecuteAsync token =
        logger.LogInformation("Started Polling Service")
        agent <- Agent.Supervised errHandle runloop () token |> Some
        agent
        |> Option.map (fun a -> a.Start())
        |> ignore
        -1
        |> Async.Sleep
        |> Async.StartAsPlainTask


    override __.Dispose() =
        base.Dispose()
        agent
        |> Option.map IDisposable.DisposeOf
        |> ignore
        client |> IDisposable.DisposeOf
        logger.LogInformation("Stopping Polling Service")
