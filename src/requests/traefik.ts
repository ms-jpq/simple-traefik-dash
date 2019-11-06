import fetch from "node-fetch"
import { REQ_TIMEOUT, API_ROUTERS } from "../consts"
import Parsimmon from "parsimmon"

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

const pEscape = Parsimmon.oneOf([`'`, `"`, "`"].join(""))

const res = pEscape.parse(`"`)

console.log(res)

const ATOMICS = {
  pHost: "",
  pPathPrefix: "",
  pOthers: "",
}
