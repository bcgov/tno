export interface IQueryStringOptions {
  /** Return query string values with 'undefined' */
  includeUndefined?: boolean;
  /** Return query string values with empty string '' */
  includeEmpty?: boolean;
  /** Convert objects to a useable value, otherwise they will use `toString()` */
  convertObject?: (value: any) => string;
}

/**
 * Converts an object into query string parameters.
 * @param params Object to convert to query parameters.
 * @param options Options to control the output query string.
 * @returns Query string parameters.
 */
export const toQueryString = (
  params: object,
  options: IQueryStringOptions = { includeUndefined: false, includeEmpty: true },
) => {
  const { includeUndefined = false, includeEmpty = true } = options;
  if (params === undefined || params === null) return '';
  const values = [];
  for (var key in params) {
    const paramKey = key;
    if (params.hasOwnProperty(paramKey)) {
      var value = (params as any)[paramKey];
      if (Array.isArray(value)) {
        values.push(
          ...value.map((v) => `${encodeURIComponent(paramKey)}=${encodeURIComponent(v)}`),
        );
      } else if (value === undefined && includeUndefined)
        values.push(`${encodeURIComponent(paramKey)}=${encodeURIComponent(value)}`);
      else if (typeof value === 'object') {
        const result = options.convertObject?.(value) ?? value;
        values.push(`${encodeURIComponent(paramKey)}=${encodeURIComponent(result)}`);
      } else if (value !== undefined && (includeEmpty || value !== ''))
        values.push(`${encodeURIComponent(paramKey)}=${encodeURIComponent(value)}`);
    }
  }
  return values.join('&');
};
