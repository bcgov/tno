import { ISortBy } from '../interfaces';

/**
 * Creates an array of sort parameters from the provided sorting information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param sortBy An array of sort objects.
 * @returns An array of sort parameters.
 */
export const applySortBy = (sortBy?: ISortBy[]) => {
  if (sortBy === undefined || sortBy.length === 0) return undefined;

  var sort: string[] = [];
  for (let i = 0; i < sortBy.length; i++) {
    let column = sortBy[i].id;
    if (column === 'section') {
      sort.push(`PrintContent.${column}${sortBy[i].desc ? ' desc' : ''}`);
      sort.push(`page${sortBy[i].desc ? ' desc' : ''}`);
    } else {
      sort.push(`${column}${sortBy[i].desc ? ' desc' : ''}`);
    }
  }
  return sort;
};
