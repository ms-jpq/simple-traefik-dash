import { ArgumentOptions, ArgumentParser } from "argparse"
import { ENV_PREFIX, READ_ME, TEXT_DIVIDER, DEFAULT_PORTS } from "./consts"
import { Pull } from "./requests/traefik"

const Parser = ((parser) => {
  const USE_ENV = ({
    env,
    defaultValue,
    required,
    ...opts
  }: ArgumentOptions & { env: string }) => {
    const envValue = process.env[env]
    if (defaultValue === undefined && envValue !== undefined) {
      defaultValue = envValue
    }
    if (required && defaultValue !== undefined) {
      required = false
    }
    return { ...opts, defaultValue, required }
  }
  parser.addArgument(
    "--traefik-api",
    USE_ENV({
      env: `${ENV_PREFIX}_TRAEFIK_API`,
      required: true,
    }),
  )
  parser.addArgument(
    "--entry-points",
    USE_ENV({
      env: `${ENV_PREFIX}_ENTRY_POINT`,
      nargs: "+",
      defaultValue: ["web-secure"],
    }),
  )
  parser.addArgument(
    "--exit-protocol",
    USE_ENV({
      env: `${ENV_PREFIX}_EXIT_PROTOCOL`,
      choices: ["http", "https"],
      defaultValue: "https",
    }),
  )
  parser.addArgument("--exit-port", USE_ENV({ env: `${ENV_PREFIX}_EXIT_PORT` }))
  parser.addArgument(
    "--label-strategy",
    USE_ENV({
      env: `${ENV_PREFIX}_LABEL_STRATEGY`,
      choices: ["as-is", "prettify"],
      defaultValue: "prettify",
    }),
  )
  parser.addArgument(
    "--ignore-routes",
    USE_ENV({
      env: `${ENV_PREFIX}_IGNORE_ROUTES`,
      nargs: "*",
      defaultValue: [],
    }),
  )

  const parse = () => {
    const { traefikApi, exitPort, ...opts } = Object.entries(
      parser.parseArgs(),
    ).reduce(
      (acc, [k, v]) =>
        Object.assign(acc, {
          [k.replace(/_\w/g, ([_, c]) => c.toUpperCase())]: v,
        }),
      {},
    ) as {
      traefikApi: string
      entryPoints: string[]
      exitProtocol: "http" | "https"
      exitPort: number
      labelStrategy: "as-is" | "prettify"
      ignoreRoutes: string[]
    }

    const { protocol, host } = new URL(traefikApi)
    return {
      ...opts,
      traefikAPI: `${protocol}//${host}`,
      exitPort: exitPort || DEFAULT_PORTS[opts.exitProtocol],
    }
  }

  const explain = () => parser.printUsage()
  return { explain, parse }
})(new ArgumentParser({}))

const Init = () => {
  console.log(READ_ME)
  Parser.explain()
  const args = Parser.parse()
  console.log(`${TEXT_DIVIDER}Current arguments:`)
  console.table(args)
  return args
}

const Main = async () => {
  // const args = Init()
  const res = await Pull("http://10.0.0.250:8080")
  // console.log(res)
}

Main()
