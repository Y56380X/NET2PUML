module Net2Puml.Uml

type Visibility =
    | Private   = 0
    | Public    = 1
    | Protected = 2
    | Package   = 3
    | Other     = 4

type Member =
    | Field  of Name: string * Visibility: Visibility * Type: string
    | Method of Name: string * Visibility: Visibility * ReturnType: string

type Element =
    | Class     of Name: string * Members: Member seq
    | Interface of Name: string * Members: Member seq

type Document = Element seq
