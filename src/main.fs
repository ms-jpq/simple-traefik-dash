namespace STD

open DomainAgnostic
open Consts
open STD.State
open STD.Env
open STD.Parsers.Rules
open DomainAgnostic.Globals
open Microsoft.Extensions.Hosting

module Entry =

    [<EntryPoint>]
    let main argv =
        if (Seq.isEmpty >> not) argv then
            argv |> Seq.iter UserPrint
        else
            echo README

            let deps = Opts()

            use state =
                new GlobalVar<State>({ lastupdate = None
                                       errors = Seq.empty
                                       ignoring = Seq.empty
                                       routes =
                                           { succ = Seq.empty
                                             fail = Seq.empty } })

            use server = Server.Build deps state
            server.Run()
        0
