open Argu
open Octokit

type Arguments =
    | [<MainCommand; ExactlyOnce>] Token of token:string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Token _ -> "GitHub API OAuth token"

let createClient token =
    let client = new GitHubClient(new ProductHeaderValue("GithubStarReleases"))
    client.Credentials <- Credentials(token)
    client

let getLastReleaseAsync (client : GitHubClient) (repository:Repository) =
    async {
        let! release = client.Repository.Release.GetAll(repository.Id) |> Async.AwaitTask
        return
            release
            |> Seq.filter (fun x -> x.PublishedAt.HasValue)
            |> Seq.sortBy (fun x -> x.PublishedAt.Value)
            |> Seq.tryLast
            |> (fun r -> r, repository)
    }

let getLastReleaseForStarredRepositories (client : GitHubClient) =
    async {
        let! stars = client.Activity.Starring.GetAllForCurrent() |> Async.AwaitTask

        return!
            stars
            |> Seq.take 20
            |> Seq.map (getLastReleaseAsync client)
            |> Async.Parallel
    }

let formatList (headers: string list) (data: seq<string list>) =
    let getMaxSize (right: string list) (left: string list) =
        (right, left) ||> List.zip  |> List.map (fun (a, b) -> if a.Length > b.Length then a else b)

    let numberOfColumns = headers.Length
    let sizes = headers::(data |> Seq.toList) |> List.reduce getMaxSize |> List.map String.length |> List.toArray
    let totalSize = (sizes |> Array.sum) + 4 + (numberOfColumns - 1) * 3
    let delimiter = new string('-', totalSize)

    let getDisplayString = List.mapi (fun i x -> sprintf " %-*s |" sizes.[i] x) >> List.reduce (+)

    printfn "%s" delimiter
    printfn "|%s" (headers |> getDisplayString)
    printfn "%s" delimiter
    data |> Seq.iter (getDisplayString >> printfn "|%s")
    printfn "%s" delimiter

let printReleases (releases: seq<Release option * Repository>) =
    releases
    |> Seq.choose (fun (release, repository) -> match release with | Some x when x.CreatedAt.Year = 2018 -> Some (x, repository) | _ -> None)
    |> Seq.map (fun (release, repository) -> [ repository.Name; release.Name; release.TagName; release.HtmlUrl ])
    |> formatList [ "Name"; "Name"; "TagName"; "Url" ]

[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<Arguments>(programName = "gsr")
        let results = parser.Parse argv
        results.GetResult Token
        |> createClient
        |> getLastReleaseForStarredRepositories
        |> Async.RunSynchronously
        |> printReleases
    with
        | :? ArguParseException as e -> printfn "%s" e.Message
    0