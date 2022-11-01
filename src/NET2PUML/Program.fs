open System
open System.IO
open System.Reflection
open System.Runtime.Loader
open Net2Puml

let args = Environment.GetCommandLineArgs ()

let inFile = FileInfo (args[1])
let outFile = FileInfo $"{Path.GetFileNameWithoutExtension inFile.Name}.puml"
let loadDependencies = true

if loadDependencies
then
    let handleAssemblyResolve (context: AssemblyLoadContext) (name: AssemblyName) =
        Path.Combine (inFile.Directory.FullName, $"{name.Name}.dll")
        |> context.LoadFromAssemblyPath
    AssemblyLoadContext.Default.add_Resolving handleAssemblyResolve

inFile.FullName
|> AssemblyLoadContext.Default.LoadFromAssemblyPath
|> UmlGenerator.ofAssembly
|> Option.map PumlGenerator.ofDocument
|> function
    | Some puml -> File.WriteAllText (outFile.FullName, puml)
    | None -> printfn "Something went wrong!"
