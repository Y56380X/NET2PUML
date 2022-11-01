module Net2Puml.PumlGenerator

open System
open System.Text
open Net2Puml.Uml

let ofMember = function
    | Field n  -> n
    | Method n -> $"{n}()"

let private ofElement' = sprintf "%s %s{\n%s}"

let private ofMembers =
    let ofMember = ofMember >> sprintf "\t%s\n"
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
