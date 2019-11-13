namespace STD.State

open System

open STD.Parsers.Traefik

type TraefikRoutes =
    { succ: Route seq
      fail: FailedRoute seq }

type State =
    { lastupdate: DateTime option
      errors: string seq
      ignoring: string seq
      routes: TraefikRoutes }
