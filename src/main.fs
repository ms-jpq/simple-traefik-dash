namespace STD

open DomainAgnostic
open Globals
open Parsers.Rules
open FParsec

module main =

    let C1 = """PathPrefix("path_prefix_1")"""
    let C2 = """Host('host_1')"""
    let C3 = """Host(`host_2`)"""

    let S1 = sprintf "%s && %s && %s" C1 C2 C3
    let S2 = sprintf "%s || %s || %s" C1 C2 C3

    let M1 = sprintf "(%s && %s) || (%s && %s)" C1 C2 C3 C1

    [<EntryPoint>]
    let main argv =
        let variables = ENV()

        let v x = x |> run expr

        echo (v C1)
        echo (v S1)
        echo (v S2)


        -1
        |> Async.Sleep
        |> Async.RunSynchronously

        0
