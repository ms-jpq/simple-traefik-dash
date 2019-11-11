namespace STD

open DomainAgnostic
open Globals
open Parsers.Rules

module main =

    let C1 = """ PathPrefix(`path_prefix_1` ) """
    let C2 = """ Host( 'host_1 '  )"""
    let C3 = """ Host("host6 " )"""

    let S1 = sprintf "%s&& %s && %s" C1 C2 C3
    let S2 = sprintf "%s || %s ||%s" C1 C2 C3

    let M1 = sprintf " ( %s && %s)|| (%s && %s ) " C1 C2 C3 C1

    let M2 = sprintf " (%s || %s) || (%s && %s) " C1 C2 C3 C1

    let M3 = sprintf "%s || %s && %s" C1 C2 C3

    let M4 = sprintf "%s && %s || %s" C1 C2 C3

    let M5 = sprintf " (%s && %s) && ( %s && %s )" C1 C2 C1 C1

    let M6 = sprintf "%s || %s || (%s &&%s)" C1 C2 C3 C1

    let test rule =
        System.Console.WriteLine()
        System.Console.WriteLine()
        System.Console.WriteLine()
        echo rule
        System.Console.WriteLine()
        let res = proutes rule
        match res with
        | Ok v -> echo v
        | Error e -> echo e
        System.Console.WriteLine()
        System.Console.WriteLine()
        System.Console.WriteLine()

    [<EntryPoint>]
    let main argv =
        let variables = ENV()
        echo "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNn"
        echo "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNn"
        echo "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNn"
        echo "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNn"
        echo "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNn"

        test C1
        test S1
        test S2
        test M1
        test M2
        test M3
        test M4
        test M5
        test M6

        -1
        |> Async.Sleep
        |> Async.RunSynchronously

        0
