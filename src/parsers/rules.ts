import P from "parsimmon"
import { FilterUnique, FlatMap, Partition } from "../domain-agnostic"

type ClauseLiteral =
  | "headers"
  | "headersregexp"
  | "host"
  | "hostregexp"
  | "method"
  | "path"
  | "pathprefix"
  | "query"

type Clause = {
  clause: ClauseLiteral
  value: string
}

type Path = {
  host: string
  pathprefix: string
}

type FailedRoute = Clause[]

const ESCAPE_CHARS = [`'`, `"`, "`"]

export const pWord = (word: string) =>
  P.seq(...[...word].map(P.string)).map((a) => a.join(""))

export const pValue = (escape: string) =>
  P.noneOf(escape)
    .atLeast(1)
    .map((s) => s.join(""))
    .wrap(P.string(escape), P.string(escape))
    .trim(P.optWhitespace)

const pValues = P.sepBy1(
  P.alt(...ESCAPE_CHARS.map(pValue)),
  P.seq(P.optWhitespace, P.string(","), P.optWhitespace),
)

export const pClause = P.seq(
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

export const pTerms = <T>(parser: P.Parser<T[]>) => {
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

export const pGroup = <T>(parser: P.Parser<T>) =>
  parser
    .wrap(
      P.seq(P.string("("), P.optWhitespace),
      P.seq(P.optWhitespace, P.string(")")),
    )
    .map((res) => [res])

export const pExpr: P.Parser<any> = P.lazy(() =>
  P.alt(pTerms(P.alt(pClause, pGroup(pExpr))), pClause),
)

export const pRoutes = (candidates: any[]) => {
  const crush = (input: any[], clauses: Clause[]): any[] => {
    const { lhs, rhs } = Partition(input, Array.isArray)
    if (lhs.length === 0) {
      return [...rhs, ...clauses]
    }
    return lhs.map((item) => crush(item, [...rhs, ...clauses]))
  }

  const extract = (input: any[], acc: any[]): Clause[][] => {
    if (input.every((item) => !Array.isArray(item))) {
      return [...acc, input]
    }
    return FlatMap(input, (item) => extract(item, acc))
  }

  const testBracket = (open: string, close: string) => (str: string) => {
    const [p1, p2] = [str.indexOf(open), str.lastIndexOf(close)]
    return p2 > p1 && p1 >= 0 && p2 >= 0
  }
  const test = testBracket("{", "}")

  const validate = (clauses: Clause[]) => {
    const candidate = clauses.reduce(
      (acc, { clause, value }) => {
        if (!acc || Reflect.has(acc, clause)) {
          return undefined
        } else {
          return Object.assign(acc, { [clause]: value })
        }
      },
      {} as Partial<Record<ClauseLiteral, string>> | undefined,
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

  const { lhs, rhs } = Partition(
    FilterUnique(
      extract(crush(candidates, []), []).map(validate),
      (a, b) => JSON.stringify(a) === JSON.stringify(b),
    ),
    ({ success }) => success,
  )

  const paths = lhs.map(({ result }) => result as Path)
  const failed = rhs.map(({ result }) => result as FailedRoute)
  return { paths, failed }
}

export const pRules = pExpr.map(pRoutes)
