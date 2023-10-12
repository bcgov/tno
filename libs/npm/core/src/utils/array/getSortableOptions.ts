import { IOptionItem, OptionItem } from '../../components';
import { ISortableModel } from '../../hooks';
import { filterEnabledOptions } from '../';

/**
 * Provides a sorting formula.
 * @param a Item one.
 * @param b Item two.
 * @returns -1 if (a < b), 0 if (a === b), 1 if (a > b).
 */
export const sortSortable = <T extends ISortableModel<any>>(a: T, b: T) => {
  if (a.sortOrder < b.sortOrder) return -1;
  if (a.sortOrder > b.sortOrder) return 1;
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  return 0;
};

/**
 * Sorts provided items into options.
 * @param items An array of items to return as sorted options.
 * @param prepend An array of options to prepend to array.
 * @param map How to map the items to options.
 * @param sort How to sort items.
 * @returns An array of options.
 */
export const getSortableOptions = <T extends ISortableModel<any>>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (i) => new OptionItem(i.name, i.id, i.isEnabled),
  sort: (a: T, b: T) => number = sortSortable,
) => {
  return prepend.concat(filterEnabledOptions([...items].sort(sort).map(map)));
};
