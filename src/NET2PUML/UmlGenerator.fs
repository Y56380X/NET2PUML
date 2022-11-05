module Net2Puml.UmlGenerator

open System.Reflection
open Net2Puml.Uml

let ofMemberInfo (m: MemberInfo) =
    match m with
    | :? FieldInfo  as f -> Some <| Field  (f.Name, f.FieldType.Name)
    | :? MethodInfo as m -> Some <| Method (m.Name, m.ReturnType.Name)
    | _                  -> None

let ofTypeInfo (t: TypeInfo) =
    let (|Class|Interface|Other|) (t: TypeInfo) =
        if t.IsClass
        then Class
        elif t.IsInterface
        then Interface
        else Other
    let umlMembers (t: TypeInfo) = Seq.map ofMemberInfo t.DeclaredMembers |> Seq.choose id
    match t with
    | Class     -> Some <| Class     (t.Name, umlMembers t)
    | Interface -> Some <| Interface (t.Name, umlMembers t)
    | Other     -> None

let ofAssembly (a: Assembly) =
    a.GetExportedTypes ()
    |> Seq.map (fun t -> t.GetTypeInfo ())
    |> Seq.map ofTypeInfo
    |> Seq.choose id
    |> Some
