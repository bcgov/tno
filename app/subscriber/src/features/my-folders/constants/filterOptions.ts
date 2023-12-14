import { getSortableOptions, IFilterModel, OptionItem } from 'tno-core';

export const getFilterOptions = (filters: IFilterModel[], currentFilterId: number) => {
  return getSortableOptions(
    filters,
    currentFilterId,
    undefined,
    (item) =>
      new OptionItem(
        `${item.name}${item.owner?.username ? ` [${item.owner.username}]` : ''}`,
        item.id,
        item.isEnabled,
      ),
  );
};
