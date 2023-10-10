import { IOptionItem, OptionItem } from '../../components';
import { IIngestModel } from '../../hooks';

export const sortIngest = <T extends IIngestModel>(a: T, b: T) => {
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  if (a.ingestTypeId < b.ingestTypeId) return -1;
  if (a.ingestTypeId > b.ingestTypeId) return 1;
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
export const getIngestOptions = <T extends IIngestModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (ds) => new OptionItem(ds.name, ds.id, ds.isEnabled),
  sort: (a: T, b: T) => number = sortIngest,
) => {
  return prepend.concat([...items].sort(sort).map(map));
};
