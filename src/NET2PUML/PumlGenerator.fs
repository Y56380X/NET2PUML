module Net2Puml.PumlGenerator

open System
open System.Text
open Net2Puml.Uml

let ofVisibility = function
    | Visibility.Private   -> "-"
    | Visibility.Public    -> "+"
    | Visibility.Protected -> "#"
    | Visibility.Other     -> String.Empty
    | _                    -> raise (ArgumentOutOfRangeException "Unknown visibility value.")

let ofMember = function
    | Field  (n, v, ft) -> $"{ofVisibility v}{n} : {ft}"
    | Method (n, v, rt) -> $"{ofVisibility v}{n}() : {rt}"

let private ofElement' t n i = $"%s{t} %s{n}{{{Environment.NewLine}%s{i}}}"

let private ofMembers =
    let ofMember m = $"\t%s{ofMember m}{Environment.NewLine}"
    Seq.map ofMember >> String.Concat

let ofElement = function
    | Class     (n, ms) -> ofElement' "class"     n (ofMembers ms)
    | Interface (n, ms) -> ofElement' "interface" n (ofMembers ms)

let ofDocument (d: Document) =
    let builder = StringBuilder ()
    builder.AppendLine "@startuml" |> ignore
    builder.AppendJoin (Environment.NewLine, Seq.map ofElement d) |> ignore
    builder.AppendLine () |> ignore
    builder.AppendLine "@enduml" |> ignore
    builder.ToString ()
