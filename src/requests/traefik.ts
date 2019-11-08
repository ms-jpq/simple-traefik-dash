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

type CLAUSE_LITERAL =
  | "headers"
  | "headersregexp"
  | "host"
  | "hostregexp"
  | "method"
  | "path"
  | "pathprefix"
  | "query"

type Clause = {
  clause: CLAUSE_LITERAL
  value: string
}

const ESCAPE_CHARS = [`'`, `"`, "`"]

const pWord = (word: string) =>
  P.seq(...[...word].map(P.string)).map((a) => a.join(""))

const pValue = (escape: string) =>
  P.noneOf(escape)
    .atLeast(1)
    .map((s) => s.join(""))
    .wrap(P.string(escape), P.string(escape))
    .trim(P.optWhitespace)

const pValues = P.sepBy1(
  P.alt(...ESCAPE_CHARS.map(pValue)),
  P.seq(P.optWhitespace, P.string(","), P.optWhitespace),
)

const pClause = P.seq(
  P.letter.atLeast(1).map((s) => s.join("").toLowerCase()),
  pValues.wrap(
    P.seq(P.string("("), P.optWhitespace),
    P.seq(P.optWhitespace, P.string(")")),
  ),
).map(([clause, values]) =>
  values.length === 1
    ? [{ clause, value: values[0] }]
    : values.map((value) => [{ clause, value }]),
)

const pTerms = <T>(parser: P.Parser<T[]>) => {
  const pSep = P.alt(pWord("&&").result(true), pWord("||").result(false)).wrap(
    P.optWhitespace,
    P.optWhitespace,
  )

  return P.seq(parser, P.seq(pSep, parser).many()).map(([fst, tail]) =>
    tail.reduce(
      (lhs, [merge, rhs]) => (merge ? [...lhs, ...rhs] : [lhs, rhs]) as any[],
      fst,
    ),
  )
}

const pGroup = <T>(parser: P.Parser<T>) =>
  parser
    .wrap(
      P.seq(P.string("("), P.optWhitespace),
      P.seq(P.optWhitespace, P.string(")")),
    )
    .map((res) => [res])

const pExpr: P.Parser<any> = P.lazy(() =>
  P.alt(pTerms(P.alt(pClause, pGroup(pExpr))), pClause),
)

const pBracket = <T>(parser: P.Parser<T>, open: string, close: string) =>
  parser.wrap(
    P.seq(P.string(open), P.optWhitespace),
    P.seq(P.optWhitespace, P.string(close)),
  )

const pTraefik = () => {}

const CLAUSE_1 = `Header("CLAUSE_1")`
const CLAUSE_2 = `Host('CLAUSE_2')`
const CLAUSE_3 = "PathPrefix(`CLAUSE_3`)"

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

TEST_PARSER(pExpr, CLAUSE_1)

TEST_PARSER(pExpr, SIMPLE_1)

TEST_PARSER(pExpr, SIMPLE_2)

TEST_PARSER(pExpr, SIMPLE_3)

TEST_PARSER(pExpr, SIMPLE_4)

TEST_PARSER(pExpr, MIXED_1)

TEST_PARSER(pExpr, MIXED_2)

TEST_PARSER(pExpr, BUCKETED_1)

TEST_PARSER(pExpr, BUCKETED_2)

TEST_PARSER(pExpr, BUCKETED_3)

TEST_PARSER(pExpr, BUCKETED_4)

TEST_PARSER(pExpr, BUCKETED_5)
