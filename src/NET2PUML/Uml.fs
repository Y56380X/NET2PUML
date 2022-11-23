module Net2Puml.Uml

type Visibility =
    | Private   = 0
    | Public    = 1
    | Protected = 2
    | Package   = 3
    | Other     = 4

type Member =
    | Field  of Name:  string * Visibility: Visibility * Type: string
    | Method of Name:  string * Visibility: Visibility * ReturnType: string

type Value =
    | Value of Value: string

type Element =
    | Class     of Name: string * Members: Member seq
    | Interface of Name: string * Members: Member seq
    | Struct    of Name: string * Members: Member seq
    | Enum      of Name: string * Values:  Value  seq

type Document = Element seq
