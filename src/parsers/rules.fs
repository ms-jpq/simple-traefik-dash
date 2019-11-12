namespace STD.Parsers

open DomainAgnostic
open FParsec
open FParsecExtensions



module Rules =


    let CLAUSELITERALS =
        [ "headers"; "headersregexp"; "host"; "hostregexp"; "method"; "path"; "pathprefix"; "query" ] |> Set

    let ESCAPECHARS = [ '''; '"'; '`' ] |> Set

    type Clause =
        { clause: string
          value: string }

        static member PrettyPrint { clause = c; value = v } = sprintf "%s(%s)" c v

    type Route = Clause seq

    type Term =
        | Routes of Route seq
        | Group of Route seq

        static member Mand t1 t2 =
            match (t1, t2) with
            | (Routes r1, Routes r2) -> Seq.Crossproduct r1 r2 |> Seq.map (fun (r1, r2) -> r1 ++ r2)
            | (Routes r, Group g) -> Seq.Crossproduct r g |> Seq.map (fun (r, g) -> g ++ r)
            | (Group g, Routes r) -> Seq.Crossproduct r g |> Seq.map (fun (r, g) -> g ++ r)
            | (Group g1, Group g2) -> Seq.Crossproduct g1 g2 |> Seq.map (fun (g1, g2) -> g1 ++ g2)
            |> Routes

        static member Mor t1 t2 =
            match (t1, t2) with
            | (Routes r1, Routes r2) -> r1 ++ r2
            | (Routes r, Group g) -> g ++ r
            | (Group g, Routes r) -> g ++ r
            | (Group g1, Group g2) -> g1 ++ g2
            |> Routes


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
        |> flip sepBy1 ((spaces .>>. pstring "," .>>. spaces) |> attempt)

    let pBracketed p = p |> between (spaces .>>. pchar '(' .>>. spaces) (spaces .>>. pchar ')' .>>. spaces)

    let clause =
        let bin (clause: string, values: string seq) =
            values
            |> Seq.map (fun v ->
                { clause = clause
                  value = v }
                |> Seq.singleton)
            |> Routes
        (letter
         |> many1Chars
         |>> (fun s -> s.ToLower())) .>>. (pBracketed values) |>> bin

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

    let pGroup parser =
        pBracketed parser |>> (fun res ->
        match res with
        | Routes r -> Group r
        | Group g -> Group g)

    let rec expr s =
        let pclause =
            clause
            |> between spaces spaces
            |> attempt

        let p = (pTerms (pclause <|> pGroup expr) <|> pclause)
        p s

    let hasBracket (str: string) =
        let p1 = str.IndexOf("{")
        let p2 = str.LastIndexOf("}")
        p2 > p1 && p1 >= 0 && p2 >= 0

    let prettyPrint route =
        route
        |> Seq.map Clause.PrettyPrint
        |> String.concat " && "

    let tryConstruct route =
        let accum { clause = c; value = v } m =
            let prev = Map.tryFind c m
            match prev with
            | Some p ->
                [ { clause = c
                    value = p }
                  { clause = c
                    value = v } ]
                |> prettyPrint
                |> sprintf "Conflicting conditions :: %s"
                |> Result.Error
            | None ->
                match (c, hasBracket v) with
                | ("pathprefix", true) ->
                    sprintf "Illegal characters, likely regex :: %s"
                        ({ clause = c
                           value = v }
                         |> Clause.PrettyPrint) |> Result.Error
                | _ -> Map.add c v m |> Result.Ok

        let construct m =
            let host = Map.tryFind "host" m

            let path =
                Map.tryFind "pathprefix" m
                |> Option.map (flip (+) "/")
                |> Option.defaultValue ""

            host
            |> Option.map (fun h ->
                { host = h
                  pathPrefix = path })
            |> Result.FromOptional "Missing host(...)"

        (Result.Ok Map.empty, route)
        ||> Seq.fold (fun m c -> (Result.bind (accum c) m))
        |> Result.bind construct
        |> Result.mapError (fun err -> (prettyPrint route, err))


    let assemble terms =
        let routes =
            match terms with
            | Group g -> g
            | Routes r -> r

        let (good, bad) =
            routes
            |> Seq.map tryConstruct
            |> Seq.fold (fun (good, bad) curr ->
                match curr with
                | Result.Ok v -> (Seq.Appending v good, bad)
                | Result.Error e -> (good, Seq.Appending e bad)) (Seq.empty, Seq.empty)

        good


    let proutes rule =
        let parse = expr |>> assemble
        rule
        |> run parse
        |> Result.FromParseResult
