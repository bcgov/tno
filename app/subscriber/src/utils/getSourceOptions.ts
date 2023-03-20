import { IOptionItem, ISourceModel, OptionItem } from 'tno-core';

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
  map: (item: T) => IOptionItem = (ds) => new OptionItem(displayName(ds), ds.id),
) => {
  return prepend.concat([...items].sort(sortSource).map(map));
};
