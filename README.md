# FSharpGlobalTools

Experiments on .NET Core global tools in F#

- [Highest grossing films](/HighestGrossingFilms): retrieve the last grossing films from [the wikipedia page](https://en.wikipedia.org/wiki/List_of_highest-grossing_films) using type providers.
- [GitHub star releases](/GitHubStarReleases): retrieve the last release for all starred repository (WIP)
- [Sln projects](/SlnProjects): list of projects in a directory and the associated solution(s)

To build and install a tool locally (from the tool folder):

- `dotnet pack --output ./`
- `dotnet tool install -g <nupkg-name> --add-source ./`