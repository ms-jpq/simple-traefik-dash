namespace STD.State

open System
open STD.Parsers.Traefik

type State =
    { lastupdate: DateTime option
      routes: TraefikRoutes }
