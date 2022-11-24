import { IQueryStringOptions, toQueryString } from './toQueryString';

/**
 * Replaces all query parameters with the parameters from the specified 'queryParams'.
 * Add the updated URL with the new query parameters to navigation history.
 * @param queryParams Query parameters.
 * @param options Query string options.
 */
export const replaceQueryParams = (queryParams: object, options?: IQueryStringOptions) => {
  const href = new URL(window.location.href.split('?')[0]);
  const search = toQueryString(queryParams, options);
  window.history.pushState({}, '', `${href}?${search}`);
};
