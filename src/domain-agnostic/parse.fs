namespace DomainAgnostic

open System

[<RequireQualifiedAccess>]
module Parse =

    let Enum<'T> str =
        try
            Enum.Parse(typedefof<'T>, str, true) :?> 'T |> Some
        with _ -> None

    let Int(str: string) =
        try
            int str |> Some
        with _ -> None

    let Long(str: string) =
        try
            int64 str |> Some
        with _ -> None

    let Float(str: string) =
        try
            float str |> Some
        with _ -> None

    let GUID(str: string) =
        try
            Guid.Parse(str.Replace("-", "")) |> Some
        with _ -> None

    let Uri(str: string) =
        try
            Uri str |> Some
        with _ -> None
