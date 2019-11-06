import { ArgumentParser, ArgumentOptions } from "argparse"
import { ParserOptions } from "webpack"

const READ_ME = `
===================================================================

Simple Traefik Dash (STD)

STD will query the Traefik API on a preset interval.

STD will generate a static dashboard of the available routes.

STD can also import more routes via JSON.

STD isnt super smart, it only supports "Host" and "PathPrefix"

===================================================================
`
console.log(READ_ME)

const USE_ENV = ({ env, defaultValue, ...opts }: ArgumentOptions & { env: string }) =>
  ({ ...opts, defaultValue: process.env[env] ?? defaultValue })


const Arguments = ((parser) => {
  parser.addArgument("--traefik-api", USE_ENV({ env: "SD_TRAEFIK_API" }))

  // parser.addArgument("--traefik-api", { action: EnvDefault })
  // parser.addArgument("--entry-point", { action: EnvDefault })
  // parser.addArgument("--prettify-labels", { action: EnvDefault })
  // parser.addArgument("--ignore", { action: EnvDefault })
  const args = Object.entries(parser.parseArgs()).reduce(
    (acc, [k, v]) => Object.assign(acc, { [k.replace(/_\w/g, ([_, c]) => c.toUpperCase())]: v }),
    {},
  )
  return args
})(new ArgumentParser({}))

console.log(Arguments)
