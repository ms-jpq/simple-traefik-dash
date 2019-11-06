import { ArgumentOptions, ArgumentParser } from "argparse"
import { ENV_PREFIX, READ_ME, TEXT_DIVIDER } from "./consts"
import { ParserOptions } from "webpack"
import { request } from "http"

const Parser = ((parser) => {
  const USE_ENV = ({ env, ...opts }: ArgumentOptions & { env: string }) => {
    const value = process.env[env]
    if (value === undefined) {
      return { ...opts }
    } else {
      return { ...opts, defaultValue: value, required: false }
    }
  }
  parser.addArgument(
    "--traefik-api",
    USE_ENV({
      env: `${ENV_PREFIX}_TRAEFIK_API`,
      defaultValue: "http://traefik:80/api/",
    }),
  )
  parser.addArgument(
    "--entry-point",
    USE_ENV({ env: `${ENV_PREFIX}SD_ENTRY_POINT`, defaultValue: "web-secure" }),
  )
  parser.addArgument(
    "--label-strategy",
    USE_ENV({
      env: `${ENV_PREFIX}_LABEL_STRATEGY`,
      choices: ["as-is", "prettify"],
      defaultValue: "prettify",
    }),
  )
  parser.addArgument(
    "--ignore",
    USE_ENV({
      env: `${ENV_PREFIX}_IGNORE`,
      type: "string",
      nargs: "*",
      defaultValue: [],
    }),
  )

  const parse = () =>
    Object.entries(parser.parseArgs()).reduce(
      (acc, [k, v]) =>
        Object.assign(acc, {
          [k.replace(/_\w/g, ([_, c]) => c.toUpperCase())]: v,
        }),
      {},
    ) as {
      traefikApi: string
      entryPoint: string
      labelStrategy: "as-is" | "prettify"
      ignore: string[]
    }

  const explain = () => parser.printUsage()
  return { explain, parse }
})(new ArgumentParser({}))

const Main = () => {
  console.log(READ_ME)
  Parser.explain()
  const args = Parser.parse()
  console.log(`${TEXT_DIVIDER}Current arguments:`)
  console.table(args)
}

Main()
