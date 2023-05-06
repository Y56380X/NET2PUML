// Copyright Y56380X https://github.com/Y56380X/NET2PUML.
// Licensed under the MIT License.

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
    let rec typeName (t: System.Type) =
        if t.IsGenericType
        then
            let name = t.Name.Substring (0, t.Name.LastIndexOf '`')
            let genericNames = Seq.map typeName t.GenericTypeArguments
            $"""{name}<{String.concat "," genericNames}>"""
        else t.Name
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
        Some <| Field  (f.Name, visibility, typeName f.FieldType)
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
        Some <| Method (m.Name, visibility, typeName m.ReturnType)
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
    let umlRelations (t: TypeInfo) =
        let isWithinAssembly (rt: System.Type) = rt <> null && rt.Assembly <> null && t.Assembly.Equals rt.Assembly
        seq {
            if isWithinAssembly t.BaseType
            then yield Inherit t.BaseType.Name
            for t in t.ImplementedInterfaces |> Seq.where isWithinAssembly do
                yield Implements t.Name
        }
    match t with
    | Class         -> Some <| Class         (t.Name, umlMembers t, umlRelations t)
    | AbstractClass -> Some <| AbstractClass (t.Name, umlMembers t, umlRelations t)
    | Interface     -> Some <| Interface     (t.Name, umlMembers t, umlRelations t)
    | Struct        -> Some <| Struct        (t.Name, umlMembers t, umlRelations t)
    | Enum          -> Some <| Enum          (t.Name, umlValues  t)
    | Other         -> None

let ofAssembly (a: Assembly) =
    a.GetExportedTypes ()
    |> Seq.map (fun t -> t.GetTypeInfo ())
    |> Utils.sortTypes
    |> Seq.map ofTypeInfo
    |> Seq.choose id
    |> Some
