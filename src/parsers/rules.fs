namespace STD.Parsers

open DomainAgnostic
open FParsec
open FParsecExtensions



module Rules =


    let CLAUSELITERALS = [ "headers"; "headersregexp"; "host"; "hostregexp"; "method"; "path"; "pathprefix"; "query" ]

    let ESCAPECHARS = [ '''; '"'; '`' ]

    type Clause =
        { clause: string
          value: string }

    type Term =
        | Base of Clause
        | Coll of Term seq

        static member Mand t1 t2 =
            match (t1, t2) with
            | (Base v1, Base v2) ->
                [ Base v1
                  Base v2 ]
                |> Seq.ofList
            | (Base v1, Coll v2) -> [ Base v1 ] ++ v2
            | (Coll v1, Base v2) -> v1 ++ [ Base v2 ]
            | (Coll v1, Coll v2) -> v1 ++ v2
            |> Coll

        static member Mor t1 t2 =
            match (t1, t2) with
            | (Base v1, Base v2) ->
                [ Coll [ Base v1 ]
                  Coll [ Base v2 ] ]
            | (Base v1, Coll v2) ->
                [ Base v1
                  Coll v2 ]
            | (Coll v1, Base v2) ->
                [ Coll v1
                  Base v2 ]
            | (Coll v1, Coll v2) ->
                [ Coll v1
                  Coll v2 ]
            |> Seq.ofList
            |> Coll


    type Path =
        { host: string
          pathPrefix: string }


    let pValue escape =
        noneOf [ escape ]
        |> many1Chars
        |> between (pchar escape) (pchar escape)
        |>> (fun s -> s.Trim())

    let values<'a> =
        ESCAPECHARS
        |> Seq.map pValue
        |> choice
        |> flip sepBy1 (spaces .>>. pstring "," .>>. spaces)

    let pBracketed p = p |> between (pchar '(' .>>. spaces) (spaces .>>. pchar ')')

    let clause: Parser<Term, unit> =
        let bin (clause, values) =
            values
            |> Seq.map (fun v ->
                { clause = clause
                  value = v }
                |> Base)
            |> Coll
        (letter |> many1Chars) .>>. (pBracketed values) |>> bin

    let pTerms parser =
        let sep = (pstring ("&&") >>% true) <|> (pstring ("||") >>% false) |> between spaces spaces

        let compact (head, tail) =
            (head, tail)
            ||> Seq.fold (fun lhs (merge, rhs) ->
                    if merge then Term.Mand lhs rhs
                    else Term.Mor lhs rhs)
        sep .>>. parser
        |> many
        |> ((.>>.) parser)
        |>> compact

    let pGroup parser = pBracketed parser |>> (fun res -> Coll [ res ])

    let rec expr s = s |> (pTerms (clause <|> pGroup expr) <|> clause)


    let crush (input: Term) (clauses: Clause seq) =

        2


    let extract input acc =
        2

    let testBracket (str: string) =
        let p1 = str.IndexOf("{")
        let p2 = str.IndexOf("}")
        p2 > p1 && p1 >= 0 && p2 >= 0

    let vaildate clauses =
        2

    let proutes (candidates: Term) =

        2
