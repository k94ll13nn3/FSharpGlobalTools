open System.IO
open Argu
open Microsoft.Build.Construction
open Tababular

type Arguments =
    | [<AltCommandLine("-d"); Unique>] Directory of path:string
    | [<AltCommandLine("-e"); Unique>] ExcludeUnique
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Directory _ -> "Path to analyze (defaults to current directory)"
            | ExcludeUnique _ -> "Exclude projects that only appear in one solution"

type Project = { Name: string; Total: int; Solutions: string list }

let parseSolutions pathToAnalyze excludeUnique =
    let getProjectsForSolution (path: string) (solution: SolutionFile) = 
        solution.ProjectsInOrder 
        |> Seq.filter (fun p -> p.ProjectType = SolutionProjectType.KnownToBeMSBuildFormat) 
        |> Seq.map (fun p -> (p.ProjectName, path))
        |> Seq.toList
    
    Directory.EnumerateFiles(pathToAnalyze, "*.sln")
    |> Seq.map (fun path -> (path |> Path.GetFileNameWithoutExtension, path |> Path.GetFullPath |> SolutionFile.Parse) ||> getProjectsForSolution)
    |> List.concat
    |> List.groupBy fst
    |> List.filter (fun (_, solutions) -> if excludeUnique then solutions.Length > 1 else true)
    |> List.map (fun (project, solutions) -> { Name = project; Total = solutions.Length; Solutions = solutions |> List.map snd })

[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<Arguments>(programName = "dotnet-slnprojects")
        let results = parser.Parse argv
        let formatter = new TableFormatter()
        let pathToAnalyze = results.GetResult (Directory, ".")
        let excludeUnique = results.Contains ExcludeUnique
        let data = parseSolutions pathToAnalyze excludeUnique
        if  data.Length > 0 then
            data |> formatter.FormatObjects |> printfn "%s"
        else
            printfn "No projects."    
    with
        | :? ArguParseException as e -> printfn "%s" e.Message
    0