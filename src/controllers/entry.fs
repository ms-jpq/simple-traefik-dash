namespace STD.Controllers

open STD.Env
open System
open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open STD.Parsers.Traefik


[<Route("/")>]
[<Controller>]
type Entry(logger: ILogger<Entry>, deps: Container<Variables>, state: GlobalVar<Route seq>) =
    inherit Controller()

    [<HttpGet("")>]
    member __.Index() =
        async {
            let! shared = state.Get()

            let data = JsonResult shared

            return data :> ActionResult
        }
        |> Async.StartAsTask
