import { IOptionItem, OptionItem } from '../../components';
import { ISourceModel } from '../../hooks';

export const sortSource = <T extends ISourceModel>(a: T, b: T) => {
  if (a.sortOrder < b.sortOrder) return -1;
  if (a.sortOrder > b.sortOrder) return 1;
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  if (a.code < b.code) return -1;
  if (a.code > b.code) return 1;
  return 0;
};

const displayName = (source: ISourceModel) => {
  let chosenName = source.shortName ? source.shortName : source.name;

  return chosenName !== source.code ? `${chosenName} (${source.code})` : chosenName;
};

/**
 * Sorts provided items into options.
 * @param items An array of items to return as sorted options.
 * @param prepend An array of options to prepend to array.
 * @param map How to map the items to options.
 * @param sort How to sort items.
 * @returns An array of options.
 */
export const getSourceOptions = <T extends ISourceModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (ds) => new OptionItem(displayName(ds), ds.id, !ds.isEnabled),
  sort: (a: T, b: T) => number = sortSource,
) => {
  return prepend.concat([...items].sort(sort).map(map));
};
