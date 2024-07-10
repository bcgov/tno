import { IUserListFilter } from 'features/admin/users/interfaces/IUserListFilter';
import { ISortBy } from 'features/interfaces';
import { IUserFilter } from 'tno-core';

/**
 * Creates a IUserFilter that can be passed to the API hook endpoint.
 * @param {IUserFilter} filter Filter object
 * @returns new IUserFilter object.
 */
export const makeUserFilter = (filter: IUserListFilter): IUserFilter => {
  return {
    ...filter,
    accountTypes: filter.accountType ? [filter.accountType] : [],
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

  var sort: string[] = [];
  for (let i = 0; i < sortBy.length; i++) {
    let column = sortBy[i].id;
    sort.push(`${column}${sortBy[i].desc ? ' desc' : ''}`);
  }
  return sort;
};
