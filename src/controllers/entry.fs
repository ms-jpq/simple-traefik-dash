namespace STD.Controllers

open STD.Env
open STD.State
open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Thoth.Json.Net
open STD.Parsers.Traefik


[<Route("")>]
type Entry(logger: ILogger<Entry>, deps: Container<Variables>, state: GlobalVar<State>) =
    inherit Controller()

    [<Route("")>]
    member __.Index() =
        async {
            let! s = state.Get()

            let res = ContentResult()
            res.Content <- s.ToString()


            return res :> ActionResult
        }
        |> Async.StartAsTask

    [<Route("status")>]
    member __.Status() =
        async {
            let! s = state.Get()
            let d = deps.Boxed
            return {| state = s
                      deps = d |} |> JsonResult :> ActionResult
        }
        |> Async.StartAsTask
