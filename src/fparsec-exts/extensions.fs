namespace FParsecExtensions

open DomainAgnostic
open FParsec
open System


[<AutoOpen>]
module Extensions =

    type Result<'T, 'E> with

        static member FromParseResult result =
            match result with
            | Success(s, _, _) -> s |> Result.Ok
            | Failure(f, _, _) -> f |> Result.ExnError

    let presult result =
        match result with
        | Success(s, _, _) -> preturn s
        | Failure(f, _, _) -> fail f

    let pbool<'a> : Parser<bool, 'a> = (pstringCI "true" >>% true) <|> (pstringCI "false" >>% false)

    let pnull<'a> : Parser<unit, 'a> = pstringCI "null" |>> ignore

    let pdate p =
        let date s =
            try
                DateTime.Parse s |> preturn
            with _ -> "Date format error" |> fail
        p >>= date

    let pguid p =
        let guid (s: string) =
            try
                Guid.Parse(s) |> preturn
            with _ -> "Guid format error" |> fail
        p >>= guid

    let chars2Str (chars: char seq) =
        chars
        |> Seq.toArray
        |> String.Concat
