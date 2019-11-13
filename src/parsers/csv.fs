namespace STD.Parsers

open DomainAgnostic
open STD.Consts
open FSharp.Data
open STD.Parsers.Traefik
open System
open System.IO

module CSV =

    let read dir = Directory.EnumerateFiles(dir) |> Seq.map (fun f -> (f, f |> (File.ReadAllText >> CsvFile.Parse)))


    let parseAdditional (file: string, csv: CsvFile) =
        let p (row: CsvRow) =
            try
                Ok
                    { name = row.["name"]
                      uris = [ row.["uri"] ]
                      location = "@std" }
            with _ -> sprintf "Expected columns - name, uri ::%s" file |> Error
        csv.Rows |> Seq.map p

    let parseIgnore (file: string, csv: CsvFile) =
        let p (row: CsvRow) =
            try
                Ok row.["name"]
            with _ -> sprintf "Expected columns - name ::%s" file |> Error
        csv.Rows |> Seq.map p


    let pCSV() =
        async {
            let (routeCSVs, e1) =
                read ROUTESDIR
                |> Seq.Bind parseAdditional
                |> Result.Discriminate

            let (blockCSVs, e2) =
                read BLOCKSDIR
                |> Seq.Bind parseIgnore
                |> Result.Discriminate

            return (routeCSVs, blockCSVs, e1 ++ e2)
        }
