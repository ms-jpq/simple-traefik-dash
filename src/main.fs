namespace STD

open DomainAgnostic
open Globals
open Consts
open System

module main =



    [<EntryPoint>]
    let main argv =
        let variables = Env.Opts()
        Console.WriteLine(README)


        -1
        |> Async.Sleep
        |> Async.RunSynchronously

        0
