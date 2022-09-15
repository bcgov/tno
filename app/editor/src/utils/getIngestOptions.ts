import { IOptionItem, OptionItem } from 'components/form';
import { IIngestModel } from 'hooks/api-editor';

export const sortIngest = <T extends IIngestModel>(a: T, b: T) => {
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  if (a.mediaTypeId < b.mediaTypeId) return -1;
  if (a.mediaTypeId > b.mediaTypeId) return 1;
  return 0;
};

export const getIngestOptions = <T extends IIngestModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (ds) => new OptionItem(ds.name, ds.id),
) => {
  return prepend.concat([...items].sort(sortIngest).map(map));
};
