import { IOptionItem, OptionItem } from '../components';
import { IIngestModel } from '../hooks';

export const sortIngest = <T extends IIngestModel>(a: T, b: T) => {
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  if (a.ingestTypeId < b.ingestTypeId) return -1;
  if (a.ingestTypeId > b.ingestTypeId) return 1;
  return 0;
};

export const getIngestOptions = <T extends IIngestModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (ds) => new OptionItem(ds.name, ds.id, ds.isEnabled),
) => {
  return prepend.concat([...items].sort(sortIngest).map(map));
};
