/**
 * A request for one page of filters on the admin filter list.
 */
export interface IFilterPageFilter {
  /** Text found in the name, description, or the owner's username, display name, first or last name. */
  keyword?: string;
  /** The page, starting at 1. */
  page: number;
  /** The filters per page. */
  quantity: number;
  /** The order, i.e. ['name'] or ['owner.username desc']. */
  sort?: string[];
}
