import { ISortBy } from 'features/interfaces';

/**
 * Creates an array of sort parameters from the provided sorting information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param sortBy An array of sort objects.
 * @returns An array of sort parameters.
 */
export const getSortBy = (sortBy?: ISortBy[]) => {
  if (sortBy === undefined || sortBy.length === 0) return undefined;

  let sort: any[] = [];
  for (let i = 0; i < sortBy.length; i++) {
    const column = sortBy[i].id;
    const value = `${sortBy[i].desc ? 'desc' : 'asc'}`;
    if (column === 'section') {
      sort.push({ page: value });
      sort.push({ [column]: value });
    } else if (column === 'product') {
      sort.push({ [column + '.name.keyword']: value });
    } else if (column === 'owner') {
      sort.push({ [column + '.username.keyword']: value });
    } else {
      sort.push({ [column]: value });
    }
  }
  return sort;
};
