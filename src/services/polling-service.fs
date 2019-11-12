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
                |> Async.Ignore
        }

    let runloop _ _ =
        async {
            do! poll()
            do! wait() |> Async.Ignore
        }

    let agent = Agent.StartSupervised errHandle runloop ()


    override __.ExecuteAsync _ =
        logger.LogInformation("Started Polling Service")
        -1
        |> Async.Sleep
        |> Async.StartAsPlainTask


    override __.Dispose() =
        agent |> IDisposable.DisposeOf
        client |> IDisposable.DisposeOf
        logger.LogInformation("Stopping Polling Service")
