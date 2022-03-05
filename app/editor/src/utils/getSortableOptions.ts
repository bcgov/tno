import { IOptionItem, OptionItem } from 'components/form';
import { ISortableModel } from 'hooks';

export const sortSortable = <T extends ISortableModel<any>>(a: T, b: T) => {
  if (a.sortOrder < b.sortOrder) return -1;
  if (a.sortOrder > b.sortOrder) return 1;
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  return 0;
};

export const getSortableOptions = <T extends ISortableModel<any>>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (i) => new OptionItem(i.name, i.id),
) => {
  return prepend.concat([...items].sort(sortSortable).map(map));
};
