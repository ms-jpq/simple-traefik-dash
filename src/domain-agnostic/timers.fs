namespace DomainAgnostic

open System
open System.Diagnostics

module Timers =

    let NewTicker period =
        let timer = Stopwatch()

        let tick() =
            async {
                let elapsed = int timer.ElapsedMilliseconds
                do! max 0 (period - elapsed) |> Async.Sleep
                timer.Restart()
                return elapsed
            }
        tick

    let Runloop period func =
        async {
            let ticker = NewTicker period
            while true do
                do! func()
                    |> Async.Catch
                    |> Async.Ignore
                do! ticker() |> Async.Ignore
        }

    let Measure log =
        let timer = Stopwatch()
        timer.Start()
        { new IDisposable with
            member __.Dispose() =
                timer.Stop()
                log timer.Elapsed }
