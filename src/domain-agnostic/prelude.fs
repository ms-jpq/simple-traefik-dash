namespace DomainAgnostic

open FSharp.Reflection
open System

[<AutoOpen>]
module Prelude =

    let echo thing = printfn "%+A" thing

    let trace thing =
        printfn "%+A" thing
        thing

    let flip f x y = f y x

    let constantly x _ = x

    let asMap (recd: 'T) =
        [ for p in FSharpType.GetRecordFields(typeof<'T>) -> p.Name, p.GetValue(recd) ]
        |> Map.ofSeq

    let tuple x y = (x, y)

    let tuple3 x y z = (x, y, z)

    let tupleApply (fx, fy) (x, y) = (fx x, fy y)

    let tuple3Apply (fx, fy, fz) (x, y, z) = (fx x, fy y, fz z)

    let (++) = Seq.append

    let CAST<'T> x =
        try
            x :> obj :?> 'T |> Some
        with _ -> None

    let ENV() =
        Environment.GetEnvironmentVariables()
        |> Seq.cast<Collections.DictionaryEntry>
        |> Seq.map (fun d -> d.Key :?> string, d.Value :?> string)
        |> Map.ofSeq


type Container<'T>(value: 'T) =
    member __.Boxed = value
