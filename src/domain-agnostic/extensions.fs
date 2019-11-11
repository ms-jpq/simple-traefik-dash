namespace DomainAgnostic

open System
open System.Threading.Tasks


[<AutoOpen>]
module Disposible =

    type Disposable = IDisposable

    type IDisposable with

        static member DisposeOf(d: #IDisposable) =
            match box d with
            | null -> ()
            | _ -> d.Dispose()

        static member Defer func =
            { new IDisposable with
                member __.Dispose() = func() }


type Agent<'T> = MailboxProcessor<'T>

[<AutoOpen>]
module Agents =

    type MailboxProcessor<'T> with

        static member Receive(inbox: MailboxProcessor<'T>) = inbox.Receive()

        static member DefaultErrHandle _ prev = async.Return prev

        static member StartSupervised errHandle processor init =
            let rec watch state inbox =
                async {
                    try
                        let! next = processor inbox state
                        return! watch next inbox
                    with e ->
                        try
                            let! next = errHandle e state
                            return! watch next inbox
                        with _ -> return! watch state inbox
                }
            watch init |> MailboxProcessor.Start

[<AutoOpen>]
module OptionalMonad =

    type Option<'T> with

        static member Recover value o =
            match o with
            | Some value -> value
            | None -> value

        static member RecoverWith func o =
            match o with
            | Some value -> value
            | None -> func()

        static member FromOptional o =
            match o with
            | ValueSome value -> Some value
            | ValueNone -> None

        static member FromResult result =
            match result with
            | Ok value -> Some value
            | Error _ -> None

        static member FromNullable x =
            match box x with
            | null -> None
            | _ -> Some x

        static member ForceUnwrap err o =
            match o with
            | Some v -> v
            | None -> failwith err

[<AutoOpen>]
module AsyncMonad =

    type Async with
        static member Return v = async.Return v

        static member New func a = async { return func a }

        static member Map func res = async {
                                         let! result = res
                                         return func result }

        static member Bind func res = async {
                                          let! result = res
                                          return! func result }

        static member BindTask (func: 'a -> Task<'b>) res =
            async {
                let! result = res
                return! result
                        |> func
                        |> Async.AwaitTask
            }

        static member StartAsPlainTask compute = compute |> Async.StartAsTask :> Task

        static member IgnoreTask task =
            task :> Task
            |> Async.AwaitTask
            |> Async.Ignore

        static member ParallelSeq a = async {
                                          let! res = a |> Async.Parallel
                                          return Seq.ofArray res }


[<AutoOpen>]
module ResultMonad =

    type Result<'T, 'E> with

        static member New func a =
            try
                func a |> Ok
            with e -> Error e

        static member ExnError msg = Exception(message = msg) |> Error

        static member FromOptional err o =
            match o with
            | Some v -> Ok v
            | None -> Error err

        static member Recover replacement result =
            match result with
            | Ok res -> res
            | Error _ -> replacement

        static member RecoverWith replace result =
            match result with
            | Ok res -> res
            | Error err -> replace err


[<RequireQualifiedAccess>]
module Seq =

    let Bind: ('a -> 'b seq) -> 'a seq -> 'b seq = Seq.collect

    let FromOptional opt =
        match opt with
        | Some v -> Seq.singleton v
        | None -> Seq.empty

    let Appending elem sequence = Seq.singleton elem |> Seq.append sequence

    let NilIfEmpty(s: 'a seq) =
        if Seq.isEmpty s then None
        else Some s

    let Partition predicate source =
        let map =
            source
            |> Seq.groupBy predicate
            |> Map.ofSeq

        let get flag =
            map
            |> Map.tryFind flag
            |> Option.defaultValue Seq.empty

        get true, get false

    let Crossproduct s1 s2 =
        seq {
            for e1 in s1 do
                for e2 in s2 do
                    yield e1, e2
        }

    let AsyncMap func sequence =
        sequence
        |> Seq.map (func |> Async.New)
        |> Async.ParallelSeq

[<RequireQualifiedAccess>]
module List =

    let FromOptional opt =
        match opt with
        | Some v -> List.singleton v
        | None -> List.empty

    let Rest lst =
        match List.length lst with
        | 0 -> []
        | _ -> List.tail lst
