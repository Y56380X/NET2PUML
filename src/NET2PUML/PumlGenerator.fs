module Net2Puml.PumlGenerator

open System
open System.Text
open Net2Puml.Uml

let ofVisibility = function
    | Visibility.Private   -> "-"
    | Visibility.Public    -> "+"
    | Visibility.Protected -> "#"
    | Visibility.Package   -> "~"
    | Visibility.Other     -> String.Empty
    | _                    -> raise (ArgumentOutOfRangeException "Unknown visibility value.")

let ofMember = function
    | Field  (n, v, ft) -> $"{ofVisibility v}{n} : {ft}"
    | Method (n, v, rt) -> $"{ofVisibility v}{n}() : {rt}"

let ofValue = function
    | Value n -> n

let private ofElement' typeName name members relations =
    $"%s{typeName} %s{name}{{{Environment.NewLine}%s{members}}}{relations}"

let private ofMembers =
    let ofMember m = $"\t%s{ofMember m}{Environment.NewLine}"
    Seq.map ofMember >> String.Concat

let private ofValues =
    let ofValue v = $"\t%s{ofValue v}{Environment.NewLine}"
    Seq.map ofValue >> String.Concat

let private ofRelations elementName =
    let ofRelation = function
        | Inherit    n -> $"{Environment.NewLine}\"{n}\" <|-- \"{elementName}\""
        | Implements n -> $"{Environment.NewLine}\"{n}\" <|-- \"{elementName}\""
    Seq.map ofRelation >> String.Concat

let ofElement = function
    | Class         (n, ms, rs) -> ofElement' "class"          n (ofMembers ms) (ofRelations n rs)
    | AbstractClass (n, ms, rs) -> ofElement' "abstract class" n (ofMembers ms) (ofRelations n rs)
    | Interface     (n, ms, rs) -> ofElement' "interface"      n (ofMembers ms) (ofRelations n rs)
    | Struct        (n, ms, rs) -> ofElement' "struct"         n (ofMembers ms) (ofRelations n rs)
    | Enum          (n, vs)     -> ofElement' "enum"           n (ofValues  vs) String.Empty

let ofDocument (d: Document) =
    let builder = StringBuilder ()
    builder.AppendLine "@startuml" |> ignore
    builder.AppendJoin (Environment.NewLine, Seq.map ofElement d) |> ignore
    builder.AppendLine () |> ignore
    builder.AppendLine "@enduml" |> ignore
    builder.ToString ()
