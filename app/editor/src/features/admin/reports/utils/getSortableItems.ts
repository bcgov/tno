import { getSortableOptions, IFilterModel, IFolderModel, OptionItem } from 'tno-core';

export const getSortableItems = <T extends IFolderModel | IFilterModel>(
  items: T[],
  currentItem?: number,
) => {
  return getSortableOptions(
    items,
    currentItem,
    [],
    (f) =>
      new OptionItem(
        `${f.name}${f.owner?.username ? ` - [${f.owner.username}]` : ''}`,
        f.id,
        !f.isEnabled,
      ),
    (a, b) => {
      if (a.owner !== undefined && b.owner !== undefined) {
        if (a.owner.username < b.owner.username) return -1;
        if (a.owner.username > b.owner.username) return 1;
      }
      if (a.sortOrder < b.sortOrder) return -1;
      if (a.sortOrder > b.sortOrder) return 1;
      if (a.name < b.name) return -1;
      if (a.name > b.name) return 1;
      return 0;
    },
  );
};
