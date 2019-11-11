namespace DomainAgnostic

open System


[<AutoOpen>]
module OptionBuilder =

    type OptionBuilder() =
        member __.Zero() = option<unit>.None

        member __.Return v = Some v

        member __.ReturnFrom(m: 'a option) = m

        member __.Bind(m, f) = Option.bind f m

        member __.TryWith(m: 'a option, f) =
            try
                m
            with e -> f e


        member __.TryFinally(m: 'a option, f) =
            try
                m
            finally
                f()


        member __.Using(m: #IDisposable, f) =
            try
                f m
            finally
                IDisposable.DisposeOf m

        member __.Delay f = f

        member __.Run f = f()

    let maybe = OptionBuilder()


[<AutoOpen>]
module ResultBuilder =

    type ResultBuilder() =
        member __.Zero() = Ok()

        member __.Return v = Ok v

        member __.ReturnFrom(v: Result<'a, 'b>) = v

        member __.Bind(m, f) = m |> Result.bind f

        member __.Using(m: #IDisposable, f) =
            try
                f m
            finally
                IDisposable.DisposeOf m

        member __.Delay f = f

        member __.Run f =
            try
                f()
            with e -> e |> Error

    let result = ResultBuilder()
