import fetch from "node-fetch"
import { REQ_TIMEOUT, API_ROUTERS } from "../consts"
import P from "parsimmon"

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

const ESCAPE_CHARS = [`'`, `"`, "`"]

const pInnie = (escape: string) =>
  P.string(escape)
    .then(P.takeWhile((c) => c !== escape))
    .skip(P.string(escape))
    .map((s) => s.trim())

const pClause = P.optWhitespace
  .then(
    P.seq(
      P.letters.map((s) => s.toLowerCase()),
      P.string("(")
        .skip(P.optWhitespace)
        .then(P.alt(...ESCAPE_CHARS.map(pInnie)))
        .skip(P.optWhitespace)
        .skip(P.string(")")),
    ).map(([clause, value]) => [{ clause, value }]),
  )
  .skip(P.optWhitespace)

const TEST_1 = ` Header(  " somethinghere   ")`
const TEST_2 = `Host(' somethinghere') `
const TEST_3 = " PathPrefix(   `somethinghere`   ) "

console.log(
  pClause.tryParse(TEST_1),
  pClause.tryParse(TEST_2),
  pClause.tryParse(TEST_3),
)

const TEST_4 = `${TEST_2} || ${TEST_3}`
const TEST_5 = `(${TEST_1} && ${TEST_2})`
const TEST_6 = `(${TEST_2} || ${TEST_3})`
