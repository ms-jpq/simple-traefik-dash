namespace STD.Controllers

open STD.Env
open STD.State
open STD.Views.TraefikServices
open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging


[<Route("")>]
type Entry(logger: ILogger<Entry>, deps: Container<Variables>, state: GlobalVar<State>) =
    inherit Controller()

    [<Route("")>]
    member self.Index() =
        async {
            let! s = state.Get()
            let html = Print deps.Boxed.title s.routes.succ
            return self.Content(html, "text/html") :> ActionResult
        }
        |> Async.StartAsTask

    [<Route("status")>]
    member __.Status() =
        async {
            let! s = state.Get()
            let d = deps.Boxed
            return {| state = s
                      ``params`` = d |} |> JsonResult :> ActionResult
        }
        |> Async.StartAsTask
