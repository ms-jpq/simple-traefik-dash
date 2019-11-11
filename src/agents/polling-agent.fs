namespace STD.Agents

open DomainAgnostic
open DomainAgnostic.Timers
open System
open Microsoft.Extensions.Logging

type PollingAgent(log: LogLevel * string * obj seq -> unit) =

    let wait = NewTicker rate

    let errHandle err prev =
        log (LogLevel.Error, "", Seq.singleton err)
        prev |> Async.Return


    let poll() =
        async {

            return 2 }



    let runloop _ _ =
        async {
            let! res = poll()
            res
            |> Result.map send
            |> Result.mapError raise
            |> ignore
            log (LogLevel.Information, ":D", Seq.empty)
            do! wait() |> Async.Ignore
            return Seq.empty
        }


    let agent = Agent.StartSupervised errHandle runloop Seq.empty

    interface IDisposable with
        member __.Dispose() =
            agent |> IDisposable.DisposeOf
            log (LogLevel.Information, "Stopped Polling")

    member __.Touch() = ()
