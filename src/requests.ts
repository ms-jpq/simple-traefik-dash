import fetch from "node-fetch"
import { REQ_TIMEOUT, API_ROUTERS, CSV_COLUMNS } from "./consts"
import { PTraefik } from "./parse/traefik"
import parse from "csv-parse"
import { readFile } from "fs"

export const PullFile = async (file: string) => {
  const data = await new Promise<Buffer>((resolve, reject) =>
    readFile(file, (err, res) => (err ? reject(err) : resolve(res))),
  )
  const csv = await new Promise<{ name: string; uri: string }>(
    (resolve, reject) =>
      parse(data, { columns: CSV_COLUMNS }, (err, res) =>
        err ? reject(err) : resolve(res),
      ),
  )
  return csv
}

export const PullTraefik = async (endPoint: string) => {
  try {
    const res = await fetch(`${endPoint}${API_ROUTERS}`, {
      timeout: REQ_TIMEOUT,
    })
    const result = await res.json()
    return { success: true, routes: PTraefik(result) }
  } catch {
    return { success: false }
  }
}
