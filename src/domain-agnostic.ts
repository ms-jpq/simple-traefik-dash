export const Partition = <T>(arr: T[], predicate: (_: T) => boolean) =>
  arr.reduce(
    ({ lhs, rhs }, curr) =>
      predicate(curr)
        ? { lhs: [...lhs, curr], rhs }
        : { lhs, rhs: [...rhs, curr] },
    { lhs: [] as T[], rhs: [] as T[] },
  )

export const FlatMap = <T, U>(arr: T[], trans: (_: T) => U[]) =>
  arr.reduce((acc, curr) => [...acc, ...trans(curr)], [] as U[])

export const FilterUnique = <T>(arr: T[], predicate: (a: T, b: T) => boolean) =>
  arr.filter((v1, i, a) => a.findIndex((v2) => predicate(v1, v2)) === i)
