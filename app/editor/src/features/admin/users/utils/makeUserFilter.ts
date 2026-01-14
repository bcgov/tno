import { type IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { type ISortBy } from 'features/interfaces';
import { type IUserFilter } from 'tno-core';

/**
 * Creates a IUserFilter that can be passed to the API hook endpoint.
 * @param {IUserFilter} filter Filter object
 * @returns new IUserFilter object.
 */
export const makeUserFilter = (filter: IUserListFilter): IUserFilter => {
  return {
    ...filter,
    accountTypes: filter.accountType != null ? [filter.accountType] : [],
    page: filter.page + 1,
    quantity: filter.quantity,
    sort: applySortBy(filter.sort),
  };
};

/**
 * Creates an array of sort parameters from the provided sorting information.
 * Cleans up the data to ensure it matches what is expected by the API.
 * @param sortBy An array of sort objects.
 * @returns An array of sort parameters.
 */
const applySortBy = (sortBy?: ISortBy[]) => {
  if (sortBy === undefined || sortBy.length === 0) return undefined;

  const sort: string[] = [];
  for (let i = 0; i < sortBy.length; i++) {
    const column = sortBy[i].id;
    sort.push(`${column}${sortBy[i].desc ? ' desc' : ''}`);
  }
  return sort;
};
