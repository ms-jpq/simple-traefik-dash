import fetch from "node-fetch"
import { REQ_TIMEOUT, API_ROUTERS } from "../consts"
import P, { Parser } from "parsimmon"

type RawRoute = {
  name: string
  status: string
  using: string[]
  rule: string
}

type Route = {
  title: string
  matched: boolean
}

const isParsable = ({ name, using, rule }: Partial<RawRoute>) => {
  const ch0 = status === "enabled"
  const chk1 = name !== undefined
  const chk2 = using !== undefined && using.length !== 0
  const chk3 = rule !== undefined
  return [ch0, chk1, chk2, chk3].every((x) => x)
}

const Filter = (candidates: Partial<RawRoute>[]) => {
  const { routes, failed } = candidates.reduce(
    ({ routes, failed }, { name, using, rule }) => {
      if (name === undefined) {
        return { routes, failed }
      }
      if (using === undefined || !using.length) {
        return { routes, failed }
      }
      return { routes, failed }
    },
    { routes: [] as Route[], failed: [] as RawRoute[] },
  )
  return { routes, failed }
}

export const Pull = async (endPoint: string) => {
  try {
    const res = await fetch(`${endPoint}${API_ROUTERS}`, {
      timeout: REQ_TIMEOUT,
    })
    const candidates = (await res.json()) as Partial<RawRoute>[]
    const { routes, failed } = Filter(candidates)
    return { success: true, routes, failed }
  } catch {
    return { success: false }
  }
}

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
  console.log(res)
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

const ESCAPE_CHARS = [`'`, `"`, "`"]

const pWord = (word: string) =>
  P.seq(...[...word].map(P.string)).map((a) => a.join(""))

const pValue = (escape: string) =>
  P.string(escape)
    .then(
      P.noneOf(escape)
        .atLeast(1)
        .map((s) => s.join("")),
    )
    .skip(P.string(escape))
    .map((s) => s.trim())

const pClause = P.seq(
  P.letter.atLeast(1).map((s) => s.join("").toLowerCase()),
  P.string("(")
    .skip(P.optWhitespace)
    .then(P.alt(...ESCAPE_CHARS.map(pValue)))
    .skip(P.optWhitespace)
    .skip(P.string(")")),
).map(([clause, value]) => [{ clause, value }])

const pGroup = <T>(parser: P.Parser<T>) =>
  P.alt(
    P.seq(P.string("("), P.optWhitespace)
      .then(parser)
      .skip(P.seq(P.optWhitespace, P.string(")"))),
    parser,
  )

const pANDsep = P.seq(P.optWhitespace, pWord("&&"), P.optWhitespace)
const pORsep = P.seq(P.optWhitespace, pWord("||"), P.optWhitespace)

const IDENTITY = <T>(x: T) => x

const FLAT_MAP = <T, U>(arr: T[], fn: (_: T) => U[]) => {
  const temp = [] as U[]
  arr.forEach((item) => temp.push(...fn(item)))
  return temp
}

const pCombo: P.Parser<any[]> = P.lazy(() =>
  P.alt(
    pClause.skip(pANDsep).chain((lhs) => pCombo.map((rhs) => [...lhs, ...rhs])),
    pClause.skip(pORsep).chain((lhs) => pCombo.map((rhs) => [lhs, rhs])),
    pClause,
  ),
)

// const pTraefik: P.Parser<any[]> = pGroup(
//   P.lazy(() => {
//     return P.alt(pClause, pAND, pOR)
//   }),
// )

const CLAUSE_1 = `Header("CLAUSE_1")`
const CLAUSE_2 = `Host('CLAUSE_2')`
const CLAUSE_3 = "PathPrefix(`CLAUSE_3`)"

const SIMPLE_1 = `${CLAUSE_1} && ${CLAUSE_2}`
const SIMPLE_2 = `${CLAUSE_1} || ${CLAUSE_2}`
const SIMPLE_3 = `${CLAUSE_1} && ${CLAUSE_2} && ${CLAUSE_3} && ${CLAUSE_1}`
const SIMPLE_$ = `${CLAUSE_1} || ${CLAUSE_2} || ${CLAUSE_3} || ${CLAUSE_1}`

const MIXED_1 = `${CLAUSE_1} && ${CLAUSE_2} || ${CLAUSE_3} && ${CLAUSE_1}`
const MIXED_2 = `${CLAUSE_1} || ${CLAUSE_2} && ${CLAUSE_3} || ${CLAUSE_1}`

/*
 *
 *
 *
 */

TEST_PARSER(pCombo, CLAUSE_1)

TEST_PARSER(pCombo, SIMPLE_1)

TEST_PARSER(pCombo, SIMPLE_2)

TEST_PARSER(pCombo, SIMPLE_3)

TEST_PARSER(pCombo, SIMPLE_$)

TEST_PARSER(pCombo, MIXED_1)

TEST_PARSER(pCombo, MIXED_2)
