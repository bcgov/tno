import { IOptionItem, OptionItem } from '../components';
import { ISourceModel } from '../hooks';

export const sortSource = <T extends ISourceModel>(a: T, b: T) => {
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  if (a.code < b.code) return -1;
  if (a.code > b.code) return 1;
  return 0;
};

const displayName = (source: ISourceModel) => {
  return source.name !== source.code ? `${source.name} (${source.code})` : source.name;
};

export const getSourceOptions = <T extends ISourceModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (ds) => new OptionItem(displayName(ds), ds.id, ds.isEnabled),
) => {
  return prepend.concat([...items].sort(sortSource).map(map));
};
