namespace STD.Controllers

open STD.Env
open System
open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

[<Controller>]
type Entry(logger: ILogger<Entry>, deps: Container<Variables>, state: GlobalVar<int>) =
    inherit Controller()

    member __.Index() =
        async {
            let! shared = state.Get()

            let data = JsonResult shared


            return data
        }
        |> Async.StartAsTask
