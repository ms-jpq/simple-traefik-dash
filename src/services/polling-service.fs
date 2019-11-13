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


type PollingService(logger: ILogger<PollingService>, deps: Container<Variables>, state: GlobalVar<State>) =
    inherit BackgroundService()

    let client =
        let c = new HttpClient()
        c.Timeout <- REQTIMEOUT
        c

    let wait = NewTicker POLLINGRATE

    let parse = materialize deps.Boxed.exitPort deps.Boxed.entryPoints

    let poll _ =
        async {
            let! _csv = pCSV() |> Async.StartChild
            let! _traefik = client.GetStringAsync(deps.Boxed.traefikAPI)
                            |> Async.AwaitTask
                            |> Async.StartChild
            let! traefik = _traefik
            let! (r1, ignore, errs) = _csv
            let (r2, failed) = parse traefik |> Result.ForceUnwrap
            let ignoring = ignore ++ deps.Boxed.ignoreRoutes |> Set

            let routes =
                r1 ++ r2
                |> Seq.filter (fun r -> (Set.contains r.name >> not) ignoring)
                |> Seq.sortBy (fun r -> r.name)

            let ns =
                { lastupdate = Some DateTime.UtcNow
                  errors = errs
                  ignoring = ignore
                  routes =
                      { succ = routes
                        fail = failed } }

            errs |> Seq.iter logger.LogError

            return! ns |> state.Put
        }

    let mutable agent = Option<Agent<unit>>.None

    let errHandle (err: exn) prev =
        logger.LogError(err.Message, err.StackTrace)
        prev |> Async.Return

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
