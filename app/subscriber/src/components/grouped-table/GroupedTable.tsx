import _ from 'lodash';
import { Column, Row, useTable } from 'react-table';

import * as styled from './styled';

export interface IGroupedTableProps<T extends object = Record<string, unknown>> {
  /** An array of column definitions. */
  columns: Column<T>[];
  /** An array of data to display. */
  data: any[];
  /** The attribute to group the data by. */
  groupBy: string;
  /** What happens on row click. */
  onRowClick?: (e: React.MouseEvent<HTMLDivElement, MouseEvent>, row: Row<T>) => void;
}

/** GroupedTable component allows the user to specify an attribute to group the data by. A header will be displayed above each grouped section containing the attributes name
 * @param {IGroupedTableProps} groupBy - The attribute to group the data by
 * @param {IGroupedTableProps} columns - The columns to display in the table
 * @param {IGroupedTableProps} data - The data to display in the table
 */
export const GroupedTable: React.FC<IGroupedTableProps> = ({
  columns,
  data,
  groupBy,
  onRowClick,
}) => {
  const { getTableProps, getTableBodyProps, headerGroups, rows, prepareRow } = useTable({
    columns,
    data,
  });

  // X can be any type, but we know it's an object
  const groups = data.map((x: any) => _.get(x, groupBy));
  // Remove duplicate sources from array
  const uniqueGroups = groups.filter((v: any, i: any, a: any) => a.indexOf(v) === i);

  return (
    <styled.GroupedTable {...getTableProps()}>
      <thead>
        {headerGroups.map((headerGroup) => (
          <tr {...headerGroup.getHeaderGroupProps()}>
            {headerGroup.headers.map((column) => (
              <th className="main-header" {...column.getHeaderProps()}>
                {column.render('Header')}
              </th>
            ))}
          </tr>
        ))}
      </thead>
      {uniqueGroups.map((group: any) => (
        <tbody {...getTableBodyProps()}>
          <tr className="groupt-title-row">
            <th className="group-title">{group}</th>
          </tr>
          {rows
            .filter((x: any) => _.get(x.original, groupBy) === group)
            .map((row) => {
              prepareRow(row);
              return (
                <tr
                  onClick={(e) => onRowClick && onRowClick(e, row)}
                  className="content-rows"
                  {...row.getRowProps()}
                >
                  {row.cells.map((cell) => {
                    return <td {...cell.getCellProps()}>{cell.render('Cell')}</td>;
                  })}
                </tr>
              );
            })}
        </tbody>
      ))}
    </styled.GroupedTable>
  );
};
