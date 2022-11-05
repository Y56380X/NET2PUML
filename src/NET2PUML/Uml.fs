module Net2Puml.Uml

type Member =
    | Field  of Name: string * Type: string
    | Method of Name: string * ReturnType: string

type Element =
    | Class     of Name: string * Members: Member seq
    | Interface of Name: string * Members: Member seq

type Document = Element seq
