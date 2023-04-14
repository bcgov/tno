import { FaFilter, FaSortDown, FaSortUp } from 'react-icons/fa';

import { ITableInternalHeaderColumn } from '.';

export interface ISortFlagProps<T extends object> {
  /** The column */
  column: ITableInternalHeaderColumn<T>;
}

/**
 * SortIndicator component provides a way to indicate the current sorting direction of the column.
 * @param param0 Header column object.
 * @returns A component to display the current sort direction.
 */
export const SortFlag = <T extends object>({ column }: ISortFlagProps<T>) => {
  return column.isSorted ? (
    <span>{column.isSortedDesc ? <FaSortDown color="blue" /> : <FaSortUp color="blue" />}</span>
  ) : column.showSort ? (
    <FaFilter className="filterable" size="10px" />
  ) : (
    <></>
  );
};
