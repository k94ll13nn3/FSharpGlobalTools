open FSharp.Data
open Argu

[<Literal>]
let Url = "https://en.wikipedia.org/wiki/List_of_highest-grossing_films"

type HighestGrossingFilms = HtmlProvider<Url>

let explore numberOfFilms =
    let data = HighestGrossingFilms.GetSample()
    data.Tables.``Highest-grossing films``.Rows
    |> Array.take numberOfFilms
    |> Array.map (fun row -> row.Title, row.``Worldwide gross``)

let formatFilms (films:(string * string) []) =
    let (maxLengthOfTitle, maxLengthOfGross) =
        films
        |> Array.fold (fun (acc1, acc2) (title, gross) -> (max title.Length acc1, max gross.Length acc2)) (0, 0)
    printfn "%s" (new string('-', maxLengthOfTitle + maxLengthOfGross + 7))
    printfn "| %-*s | %-*s |" maxLengthOfTitle "Title" maxLengthOfGross "Gross"
    printfn "%s" (new string('-', maxLengthOfTitle + maxLengthOfGross + 7))
    films |> Array.iter (fun (title, gross) -> printfn "| %-*s | %*s |" maxLengthOfTitle title maxLengthOfGross gross)
    printfn "%s" (new string('-', maxLengthOfTitle + maxLengthOfGross + 7))

type Arguments =
    | [<MainCommand; ExactlyOnce>] Number of number:int
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Number _ -> "number of highest grossing films"

[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<Arguments>(programName = "hgf")
        let results = parser.Parse argv
        results.GetResult Number |> explore |> formatFilms
    with
        | :? ArguParseException as e -> printfn "%s" e.Message
    0