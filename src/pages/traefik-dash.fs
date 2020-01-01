namespace STD.Views

open DomainAgnostic
open Giraffe.GiraffeViewEngine
open STD.Parsers.Traefik
open STD.Consts


module Dashboard =

    let css = sprintf "body { background-image: url(%s); }"

    let Page background tit bdy =
        html []
            [ head []
                  [ meta [ _charset "utf-8" ]
                    meta
                        [ _name "viewport"
                          _content "width=device-width, initial-scale=1" ]
                    link
                        [ _rel "stylesheet"
                          _href "site.css" ]
                    style []
                        [ background
                          |> css
                          |> str ]
                    script
                        [ _async
                          _defer
                          _src "script.js" ] []
                    title [] [ str tit ] ]
              body [] bdy ]

    let Layout contents =
        div []
            [ main [] contents
              footer []
                  [ a [ _href "status" ] [ str "API" ]
                    a [ _href PROJECTURI ] [ str "github" ] ] ]
        |> List.singleton

    let Route uri (name: string) =
        let pretty = name.Replace('-', ' ').Replace('_', ' ')

        let big =
            name
            |> Seq.truncate 2
            |> Seq.map string
            |> String.concat ""

        figure []
            [ a [ _href uri ] [ h1 [] [ str big ] ]
              figcaption [] [ a [ _href uri ] [ str pretty ] ] ]

    let disperse { name = n; uris = us } =
        match us |> List.ofSeq with
        | [ u ] -> Route u n |> List.singleton
        | u -> u |> List.mapi (fun i u -> sprintf "%s-%d" n i |> Route u)


    let Render background tit routes =
        routes
        |> List.ofSeq
        |> List.Bind disperse
        |> Layout
        |> Page background tit
        |> renderHtmlDocument
