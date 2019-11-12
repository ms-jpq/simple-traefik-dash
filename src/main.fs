namespace STD

open DomainAgnostic
open Consts
open System
open STD.Env
open DomainAgnostic.Globals
open STD.Parsers.Traefik
open Microsoft.Extensions.Hosting

module Entry =

    [<EntryPoint>]
    let main argv =
        echo README

        let deps = Opts()
        use state = new GlobalVar<Route seq>(Seq.empty)
        use server = Server.Build deps state
        server.Run()
        echo TEXTDIVIDER

        0
