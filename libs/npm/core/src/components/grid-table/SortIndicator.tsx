import { FaFilter, FaSortDown, FaSortUp } from 'react-icons/fa';
import { HeaderGroup } from 'react-table';

import { Show } from '../show';

export interface ISortIndicatorProps<T extends object> {
  /**
   * Header column object.
   */
  column: HeaderGroup<T>;
}

/**
 * SortIndicator component provides a way to indicate the current sorting direction of the column.
 * @param param0 Header column object.
 * @returns A component to display the current sort direction.
 */
export const SortIndicator = <T extends object>({ column }: ISortIndicatorProps<T>) => {
  return column.isSorted ? (
    <span>{column.isSortedDesc ? <FaSortDown color="blue" /> : <FaSortUp color="blue" />}</span>
  ) : (
    <Show visible={column.canSort}>
      <FaFilter className="filterable" size="10px" />
    </Show>
  );
};
