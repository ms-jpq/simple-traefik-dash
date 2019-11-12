namespace STD.Services

open DomainAgnostic
open DomainAgnostic.Timers
open STD.Env
open STD.Consts
open STD.Parsers.Traefik
open System
open Microsoft.Extensions.Logging
open DomainAgnostic.Globals
open Microsoft.Extensions.Hosting
open System.Net.Http

type PollingService(logger: ILogger<PollingService>, deps: Container<Variables>, state: GlobalVar<Route seq>) =
    inherit BackgroundService()

    let client =
        let c = new HttpClient()
        c.Timeout <- REQTIMEOUT
        c

    let wait = NewTicker POLLINGRATE

    let parse = materialize deps.Boxed.exitPort deps.Boxed.entryPoints deps.Boxed.ignoreRoutes

    let errHandle (err: exn) prev =
        logger.LogError(err.Message, err.StackTrace)
        prev |> Async.Return

    let poll _ =
        async {
            let! response = client.GetStringAsync(deps.Boxed.traefikAPI) |> Async.AwaitTask
            do! parse response
                |> Result.ForceUnwrap
                |> state.Put
        }

    let runloop _ _ =
        async {
            do! poll()
            do! wait() |> Async.Ignore
        }

    let mutable agent = Option<Agent<unit>>.None

    override __.ExecuteAsync token =
        logger.LogInformation("Started Polling Service")
        agent <- Agent.Supervised errHandle runloop () token |> Some
        agent |> Option.map (fun a -> a.Start()) |> ignore
        -1
        |> Async.Sleep
        |> Async.StartAsPlainTask


    override __.Dispose() =
        base.Dispose()
        agent |> Option.map IDisposable.DisposeOf |> ignore
        client |> IDisposable.DisposeOf
        logger.LogInformation("Stopping Polling Service")
