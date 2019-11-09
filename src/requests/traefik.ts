import fetch from "node-fetch"
import { REQ_TIMEOUT, API_ROUTERS } from "../consts"
import P from "parsimmon"
import { deepEqual } from "assert"

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
const partition = <T>(arr: T[], predicate: (_: T) => boolean) =>
  arr.reduce(
    ({ lhs, rhs }, curr) =>
      predicate(curr)
        ? { lhs: [...lhs, curr], rhs }
        : { lhs, rhs: [...rhs, curr] },
    { lhs: [] as T[], rhs: [] as T[] },
  )

const flatMap = <T, U>(arr: T[], trans: (_: T) => U[]) =>
  arr.reduce((acc, curr) => [...acc, ...trans(curr)], [] as U[])

const filterUnique = <T>(arr: T[], predicate: (a: T, b: T) => boolean) =>
  arr.filter((v1, i, a) => a.findIndex((v2) => predicate(v1, v2)) === i)

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

type Path = {
  domain: string
  prefix: string
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

const pRoutes = (candidates: any[]) => {
  const crush = (input: any[], clauses: Clause[]): any[] => {
    const { lhs, rhs } = partition(input, Array.isArray)
    if (lhs.length === 0) {
      return [...rhs, ...clauses]
    }
    return lhs.map((item) => crush(item, [...rhs, ...clauses]))
  }

  const extract = (input: any[], acc: any[]): Clause[][] => {
    if (input.every((item) => !Array.isArray(item))) {
      return [...acc, input]
    }
    return flatMap(input, (item) => extract(item, acc))
  }

  const testBracket = (open: string, close: string) => (str: string) => {
    const [p1, p2] = [str.indexOf(open), str.lastIndexOf(close)]
    return p2 > p1 && p1 >= 0 && p2 >= 0
  }

  const validate = (clauses: Clause[]) => {
    const test = testBracket("{", "}")
    const candidate = clauses.reduce(
      (acc, { clause, value }) => {
        if (!acc || Reflect.has(acc, clause)) {
          return undefined
        } else {
          return Object.assign(acc, { [clause]: value })
        }
      },
      {} as Partial<Record<CLAUSE_LITERAL, string>> | undefined,
    )

    const { host, pathprefix, ...opts } = candidate || {}
    if (!host || (pathprefix && test(pathprefix)) || Object.keys(opts).length) {
      return {
        success: false,
        result: clauses,
      }
    }
    const result = { host, pathprefix: pathprefix || "" }
    return { success: true, result }
  }

  const { lhs, rhs } = partition(
    filterUnique(
      extract(crush(candidates, []), []).map(validate),
      (a, b) => JSON.stringify(a) === JSON.stringify(b),
    ),
    ({ success }) => success,
  )

  const paths = lhs.map(({ result }) => result)
  const failed = rhs.map(({ result }) => result)
  return { paths, failed }
}

const pTraefik = pExpr.map(pRoutes)

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

TEST_PARSER(pTraefik, CLAUSE_1)

TEST_PARSER(pTraefik, SIMPLE_1)

TEST_PARSER(pTraefik, SIMPLE_2)

TEST_PARSER(pTraefik, SIMPLE_3)

TEST_PARSER(pTraefik, SIMPLE_4)

TEST_PARSER(pTraefik, MIXED_1)

TEST_PARSER(pTraefik, MIXED_2)

TEST_PARSER(pTraefik, BUCKETED_1)

TEST_PARSER(pTraefik, BUCKETED_2)

TEST_PARSER(pTraefik, BUCKETED_3)

TEST_PARSER(pTraefik, BUCKETED_4)

TEST_PARSER(pTraefik, BUCKETED_5)
