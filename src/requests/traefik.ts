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

const TST = (test: any, name = INC()) => {
  console.log(`
  =======================================================
  TEST - ${name}
  `)
  const res = (typeof test === "function" && test) || test
  console.log(res)
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

const pAND = <T>(parser: P.Parser<T[]>, prev: any[]) =>
  pANDsep.then(parser).map((curr) => [...prev, ...curr])

const pOR = <T>(parser: P.Parser<T[]>, prev: any[]) =>
  pORsep.then(parser).map((curr) => [prev, curr])

const pCombo: P.Parser<any[]> = P.lazy(() =>
  P.alt(
    pClause.chain((res) =>
      P.alt(pAND(pClause, res), pOR(pClause, res), pClause),
    ),
    pClause,
  ),
)

// const pTraefik: P.Parser<any[]> = pGroup(
//   P.lazy(() => {
//     return P.alt(pClause, pAND, pOR)
//   }),
// )

const TEST_1 = `Header(  " test_1   ")`
const TEST_2 = `Host(' test_2')`
const TEST_3 = "PathPrefix(   `test_3`   )"

// console.log(
//   pGroup(pClause).tryParse(TEST_1),
//   pGroup(pClause).tryParse(TEST_2),
//   pGroup(pClause).tryParse(TEST_3),
// )

const TEST_4 = `${TEST_1} && ${TEST_2}`
const TEST_5 = `${TEST_1} || ${TEST_2}`
const TEST_6 = `${TEST_1} && ${TEST_2} && ${TEST_3}`
const TEST_7 = `${TEST_1} || ${TEST_2} || ${TEST_3}`

TST(pCombo.tryParse(TEST_1))

TST(pCombo.tryParse(TEST_4))

TST(pCombo.tryParse(TEST_5))

TST(pCombo.tryParse(TEST_6))

TST(pCombo.tryParse(TEST_7))
