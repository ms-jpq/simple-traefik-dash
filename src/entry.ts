import { ArgumentOptions, ArgumentParser } from "argparse"
import { ENV_PREFIX, READ_ME, TEXT_DIVIDER, DEFAULT_PORTS } from "./consts"
import P from "parsimmon"
import { pRules } from "./parse/rules"
import { PullTraefik, PullFile } from "./requests"

type Args = {
  traefikApi: string
  entryPoints: string[]
  exitProtocol: "http" | "https"
  exitPort: number
  labelStrategy: "as-is" | "prettify"
  ignoreRoutes: string[]
}

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
    ) as Args

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

const init = () => {
  console.log(READ_ME)
  Parser.explain()
  const args = Parser.parse()
  console.log(`${TEXT_DIVIDER}Current arguments:`)
  console.table(args)
  return args
}

const pull = () => {}

const Main = async () => {
  // const args = Init()
  const res = await PullTraefik("http://10.0.0.250:8080")
  // console.log(res)
}

Main()

/*
 *
 *
 *
 *
 *
 *
 *
 *
 *
 */

const AutoInc = () => ((n) => () => n++)(1)
const INC = AutoInc()

const TST = (test: any, name: string | number = INC()) => {
  console.log(`
  =======================================================
  TEST - ${name}
  `)
  const res = (typeof test === "function" && test()) || test
  console.log(JSON.stringify(res, undefined, 2))
}

const TEST_PARSER = <T>(parser: P.Parser<T>, str: string) =>
  TST(() => parser.tryParse(str), str)

/*
 *
 *
 *
 *
 *
 *
 *
 *
 *
 */

const CLAUSE_1 = `PathPrefix("path_prefix_1a", 'path_prefix_1b')`
const CLAUSE_2 = `Host('host_1')`
const CLAUSE_3 = "Header(`header_1`)"

const SIMPLE_1 = `${CLAUSE_1} && ${CLAUSE_2}`
const SIMPLE_2 = `${CLAUSE_1} || ${CLAUSE_2}`
const SIMPLE_3 = `${CLAUSE_1} && ${CLAUSE_2} && ${CLAUSE_3} && ${CLAUSE_1}`
const SIMPLE_4 = `${CLAUSE_1} || ${CLAUSE_2} || ${CLAUSE_3} || ${CLAUSE_1}`

const MIXED_1 = `${CLAUSE_1} && ${CLAUSE_2} || ${CLAUSE_3} && ${CLAUSE_1}`
const MIXED_2 = `${CLAUSE_1} || ${CLAUSE_2} && ${CLAUSE_3} || ${CLAUSE_1}`

const BUCKETED_1 = `(${CLAUSE_1}) || (${CLAUSE_2})`
const BUCKETED_2 = `(${CLAUSE_1}) && (${CLAUSE_2})`
const BUCKETED_3 = `(${CLAUSE_1}) && (${CLAUSE_2} && ${CLAUSE_3})`
const BUCKETED_4 = `(${CLAUSE_1}) && (${CLAUSE_2} || (${CLAUSE_3} && ${CLAUSE_1}))`
const BUCKETED_5 = `(${CLAUSE_1}) && (${CLAUSE_2} || (${CLAUSE_3} && (${CLAUSE_1} || ${CLAUSE_2})))`

/*
 *
 *
 *
 */

TEST_PARSER(pRules, CLAUSE_1)

TEST_PARSER(pRules, SIMPLE_1)

TEST_PARSER(pRules, SIMPLE_2)

TEST_PARSER(pRules, SIMPLE_3)

TEST_PARSER(pRules, SIMPLE_4)

TEST_PARSER(pRules, MIXED_1)

TEST_PARSER(pRules, MIXED_2)

TEST_PARSER(pRules, BUCKETED_1)

TEST_PARSER(pRules, BUCKETED_2)

TEST_PARSER(pRules, BUCKETED_3)

TEST_PARSER(pRules, BUCKETED_4)

TEST_PARSER(pRules, BUCKETED_5)
