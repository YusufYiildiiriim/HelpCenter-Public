/**
 * Single-point query-param normalization for `api.get(url, { params: buildQueryParams({...}) })`.
 * undefined/null/empty strings are removed, booleans/numbers are converted to strings,
 * Date values are converted to ISO strings, and array elements are filtered and serialized individually.
 *
 * Array values are sent as `key=v1&key=v2` (repeated key, without brackets) —
 * see `paramsSerializer: { indexes: null }` in `src/lib/axios.ts`, which is the format
 * expected by backend's [FromQuery] List<T>/T[] binding (axios's default produces
 * `key[]=`, which ASP.NET Core does not bind).
 */

export type QueryParamPrimitive = string | number | boolean | Date;
export type QueryParamValue = QueryParamPrimitive | null | undefined;
export type QueryParamInput = QueryParamValue | QueryParamPrimitive[];

export type BuiltQueryParams = Record<string, string | string[]>;

function serializePrimitive(value: QueryParamPrimitive): string {
  if (value instanceof Date) return value.toISOString();
  return String(value);
}

export function buildQueryParams(params: Record<string, QueryParamInput>): BuiltQueryParams {
  const result: BuiltQueryParams = {};

  for (const [key, rawValue] of Object.entries(params)) {
    if (Array.isArray(rawValue)) {
      const serialized = rawValue
        .filter((item): item is QueryParamPrimitive => item !== null && item !== undefined)
        .map(serializePrimitive)
        .filter((item) => item.length > 0);
      if (serialized.length > 0) result[key] = serialized;
      continue;
    }

    if (rawValue === null || rawValue === undefined) continue;

    const serialized = serializePrimitive(rawValue);
    if (serialized.length > 0) result[key] = serialized;
  }

  return result;
}
