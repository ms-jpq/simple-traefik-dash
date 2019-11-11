namespace STD.Parsers

open Rules

module Traefik =

    type RawRoute =
        { name: string
          status: string
          using: string []
          rule: string
          tls: bool option }

    
