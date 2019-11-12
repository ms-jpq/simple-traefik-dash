namespace STD.Services

open DomainAgnostic
open DomainAgnostic.Timers
open STD.Env
open STD.Consts
open System
open Microsoft.Extensions.Logging
open DomainAgnostic.Globals
open Microsoft.Extensions.Hosting
open System.Net.Http
open System.Text.Json


type PollingService(logger: ILogger<PollingService>, deps: Container<Variables>, state: GlobalVar<int>) =

    inherit BackgroundService()

    let client =
        let c = new HttpClient()
        c.Timeout <- REQTIMEOUT
        c

    let wait = NewTicker POLLINGRATE

    let errHandle err prev =
        logger.LogError("", [ err ])
        prev |> Async.Return

    let poll() =
        async {
            let! response = client.GetStringAsync(deps.Boxed.traefikAPI) |> Async.AwaitTask



            return 0 }

    let runloop _ _ =
        async {



            do! wait() |> Async.Ignore
            return ()
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
        logger.LogInformation("Stopping Polling Service")
