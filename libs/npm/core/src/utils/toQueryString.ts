/**
 * Converts an object into query string parameters.
 * @param params Object to convert to query parameters.
 * @returns Query string parameters.
 */
export const toQueryString = (params: object) => {
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
      } else if (value !== undefined)
        values.push(`${encodeURIComponent(paramKey)}=${encodeURIComponent(value)}`);
    }
  }
  return values.join('&');
};
