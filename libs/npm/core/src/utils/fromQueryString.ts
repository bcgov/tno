export interface IQueryOptions {
  /** An array of query parameters that should be converted to arrays. */
  arrays?: string[];
  /** An array of query parameters that should be converted to numbers. */
  numbers?: string[];
}

/**
 * Converts a query string to an object.
 * @param query The query string to convert to an object.
 * @param options Query options to control the output.
 * @returns A new instance of an object.
 */
export const fromQueryString = (query?: string, options?: IQueryOptions): any => {
  return query
    ? (/^[?#]/.test(query) ? query.slice(1) : query).split('&').reduce((params, param) => {
        let [key, value] = param.split('=');

        var decodedValue: string | number = value
          ? decodeURIComponent(value.replace(/\+/g, ' '))
          : '';
        if (options?.numbers?.includes(key)) {
          decodedValue = Number(decodedValue);
        }

        // If the output object already contains the key then determine if it's an array.
        var currentValue = (params as any)[key];
        var isArray = options?.arrays?.includes(key) ?? false;

        if (currentValue === undefined && !isArray) (params as any)[key] = decodedValue;
        else if (currentValue === undefined && isArray) (params as any)[key] = [decodedValue];
        else if ((decodedValue !== '' && Array.isArray(currentValue)) || isArray) {
          (params as any)[key] = [...currentValue, decodedValue];
        } else if (decodedValue !== '') {
          // There are multiple values assigned to the same key.  Convert to an array.
          (params as any)[key] = [currentValue, decodedValue];
        }

        return params;
      }, {})
    : {};
};
