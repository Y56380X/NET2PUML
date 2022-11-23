module Net2Puml.UmlGenerator

open System.Reflection
open Net2Puml.Uml

module private Utils =
    let sortTypes (ts: TypeInfo seq) : TypeInfo seq =
        let rec inheritanceCounter (t: System.Type) =
            if t.BaseType = null || t.BaseType.Assembly <> t.Assembly
            then 0
            else inheritanceCounter t.BaseType
        Seq.sortBy inheritanceCounter ts

let ofMemberInfo (m: MemberInfo) =
    match m with
    | :? FieldInfo  as f ->
        let visibility =
            if f.IsPrivate
            then Visibility.Private
            elif f.IsPublic
            then Visibility.Public
            elif f.IsFamily
            then Visibility.Protected
            elif f.IsAssembly
            then Visibility.Package
            else Visibility.Other
        Some <| Field  (f.Name, visibility, f.FieldType.Name)
    | :? MethodInfo as m ->
        let visibility =
            if m.IsPrivate
            then Visibility.Private
            elif m.IsPublic
            then Visibility.Public
            elif m.IsFamily
            then Visibility.Protected
            elif m.IsAssembly
            then Visibility.Package
            else Visibility.Other
        Some <| Method (m.Name, visibility, m.ReturnType.Name)
    | _ -> None

let ofTypeInfo (t: TypeInfo) =
    let (|Class|AbstractClass|Interface|Struct|Enum|Other|) (t: TypeInfo) =
        if t.IsClass
        then if t.IsAbstract then AbstractClass else Class
        elif t.IsInterface
        then Interface
        elif t.IsEnum
        then Enum
        elif t.IsValueType
        then Struct
        else Other
    let umlMembers (t: TypeInfo) = Seq.map ofMemberInfo t.DeclaredMembers |> Seq.choose id
    let umlValues : TypeInfo -> Value seq = System.Enum.GetNames >> Seq.map Value
    match t with
    | Class         -> Some <| Class         (t.Name, umlMembers t)
    | AbstractClass -> Some <| AbstractClass (t.Name, umlMembers t)
    | Interface     -> Some <| Interface     (t.Name, umlMembers t)
    | Struct        -> Some <| Struct        (t.Name, umlMembers t)
    | Enum          -> Some <| Enum          (t.Name, umlValues  t)
    | Other         -> None

let ofAssembly (a: Assembly) =
    a.GetExportedTypes ()
    |> Seq.map (fun t -> t.GetTypeInfo ())
    |> Utils.sortTypes
    |> Seq.map ofTypeInfo
    |> Seq.choose id
    |> Some
