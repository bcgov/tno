/**
 * Converts a query string to an object.
 * @param query The query string to convert to an object.
 * @returns A new instance of an object.
 */
export const fromQueryString = (query?: string): any => {
  return query
    ? (/^[?#]/.test(query) ? query.slice(1) : query).split('&').reduce((params, param) => {
        let [key, value] = param.split('=');

        var currentValue = (params as any)[key];
        if (currentValue === undefined)
          (params as any)[key] = value ? decodeURIComponent(value.replace(/\+/g, ' ')) : '';
        else if (Array.isArray(currentValue)) {
          const newValue = value ? decodeURIComponent(value.replace(/\+/g, ' ')) : undefined;
          if (newValue !== undefined) (params as any)[key] = [...currentValue, newValue];
        } else {
          const newValue = value ? decodeURIComponent(value.replace(/\+/g, ' ')) : undefined;
          if (newValue !== undefined) (params as any)[key] = [currentValue, newValue];
        }

        return params;
      }, {})
    : {};
};
