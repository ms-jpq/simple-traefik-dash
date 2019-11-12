namespace STD.Parsers

open DomainAgnostic
open Rules
open Thoth.Json.Net
open System

module Traefik =

    type RawRoute =
        { name: string
          enabled: bool
          using: string Set
          rule: string
          tls: bool }

        static member Decoder =
            Decode.object (fun get ->
                { name = get.Required.Field "name" Decode.string
                  enabled = (get.Required.Field "status" Decode.string).ToLower() = "enabled"
                  using = get.Required.Field "using" (Decode.array Decode.string) |> Set
                  rule = get.Required.Field "rule" Decode.string
                  tls = (get.Optional.Field "tls" Decode.value).IsSome })

    type Route =
        { name: string
          uris: Uri seq }

    let decode = Decode.array RawRoute.Decoder |> Decode.fromString

    let materialize port using ignored json =
        result {
            let! candidates = decode json |> Result.mapError Exception
            let (good, bad) =
                candidates
                |> Seq.filter
                    (fun c ->
                    c.enabled && Set.intersect c.using using |> (Set.isEmpty >> not)
                    && (Set.contains c.name >> not) ignored)
                |> Seq.fold (fun (g, b) c ->
                    let res = proutes c.rule
                    match res with
                    | Ok rs -> (Seq.Appending (c, rs) g, b)
                    | Error e -> (g, Seq.Appending e b)) (Seq.empty, Seq.empty)

            let res =
                good
                |> Seq.map (fun (r, rs) ->
                    { name = r.name
                      uris =
                          Seq.map (fun s ->
                              let protocol =
                                  if r.tls then "https"
                                  else "http"
                              sprintf "%s://%s:%d%s" protocol s.host port s.pathPrefix |> Uri) rs })

            return res
        }
