namespace STD.APIS

open STD.Env
open System
open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

module Entry =

    [<Route("/")>]
    [<Controller>]
    type MetricsClient(logger: ILogger<MetricsClient>, deps: Container<Variables>, state: GlobalVar<int>) =
        inherit Controller()


        [<HttpGet("/")>]
        member __.Value() =
            async {
                let! shared = state.Get()

                let data = JsonResult shared


                return data
            }
            |> Async.StartAsTask
