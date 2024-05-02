import React from 'react';

import { Container } from '../container';
import { Text } from '../form';
import { getSortId, ITableProps, SortFlag, TablePager, useTable } from '.';
import * as styled from './styled';
import { determineSortValue } from './utils/determineSort';
export const FlexboxTable = <T extends object>({
  rowId,
  columns,
  data,
  isLoading,
  showActive = true,
  disableZebraStriping,
  ...rest
}: ITableProps<T>) => {
  const table = useTable({
    rowId,
    columns,
    data,
    options: {
      isMulti: rest.isMulti,
      onRowClick: rest.onRowClick,
      onCellClick: rest.onCellClick,
      onColumnClick: rest.onColumnClick,
      onSelectedChanged: rest.onSelectedChanged,
      onKeyDown: rest.onKeyDown,
      stopPropagation: rest.stopPropagation,
      showHeader: rest.showHeader,
      showFooter: rest.showFooter,
      selectedRowIds: rest.selectedRowIds,
      activeRowId: rest.activeRowId,
    },
    paging: {
      pagingEnabled: rest.pagingEnabled,
      manualPaging: rest.manualPaging,
      pageIndex: rest.pageIndex,
      pageSize: rest.pageSize,
      pageCount: rest.pageCount,
      totalItems: rest.totalItems,
      pageButtons: rest.pageButtons,
      showPaging: rest.showPaging,
      scrollSize: rest.scrollSize,
      onPageChange: rest.onPageChange,
    },
    sorting: {
      showSort: rest.showSort,
      sortOrder: rest.sortOrder,
      onSortChange: rest.onSortChange,
    },
    filter: {
      showFilter: rest.showFilter,
      search: rest.search,
      onFilterChange: rest.onFilterChange,
    },
    grouping: {
      groupBy: rest.groupBy,
      groupHeading: rest.groupHeading,
    },
  });
  const [search, setSearch] = React.useState(table.search);
  const _rowId = typeof rowId === 'function' ? rowId : (data?: T) => `${data?.[rowId]}`;

  const style = {
    className: `table${rest.className ? ` ${rest.className}` : ''}`,
    columns: table.columns,
    scrollSize: table.scrollSize,
    disableZebraStriping: disableZebraStriping,
  };

  return (
    <styled.FlexboxTable {...style}>
      <Container isLoading={isLoading}>
        {table.showFilter && (
          <div className="filter">
            <div>
              <Text
                name="filter"
                placeholder="Search by keyword"
                value={search}
                onChange={(e) => {
                  setSearch(e.target.value);
                  table.applyFilter(e.target.value);
                }}
              />
            </div>
          </div>
        )}
        {table.options.showHeader && (
          <header className="header">
            {table.header.columns
              .filter((col) => col.isVisible)
              .map((col, index) => (
                <div
                  key={col.index}
                  className={`column col-${index}`}
                  onClick={(e) => {
                    if (table.options.stopPropagation) e.stopPropagation();
                    table.options.onColumnClick?.(table.columns[index], e);
                  }}
                >
                  {col.label && <span className="label">{col.label}</span>}
                  {col.label && col.label !== 'Remove' && table.showSort && (
                    <div
                      className="sort"
                      onClick={() => {
                        table.setSortOrder([
                          ...table.sortOrder.filter((sort) => sort.id !== getSortId(col, index)),
                          {
                            id: getSortId(col, index),
                            index: index,
                            sort: determineSortValue(col),
                            isSorted: !col.isSorted ? true : col.isSortedDesc ? false : true,
                            isSortedDesc: col.isSorted ? !col.isSortedDesc : col.isSortedDesc,
                          },
                        ]);
                      }}
                    >
                      <SortFlag column={col} />
                    </div>
                  )}
                </div>
              ))}
          </header>
        )}
        <div className="rows">
          {table.groupBy && (
            <div className="groups">
              {table.groups.map((group) => {
                return (
                  <React.Fragment key={group.key}>
                    <div className="group">{group.key}</div>
                    <div className="group-rows">
                      {group.rows.map((row) => {
                        return (
                          <div
                            className={`row${row.isSelected ? ' selected' : ''}${
                              showActive && row.isActive ? ' active' : ''
                            }`}
                            key={_rowId(row.original)}
                            onClick={(e) => {
                              if (table.options.stopPropagation) e.stopPropagation();
                              table.options.onRowClick?.(row, e);
                            }}
                            tabIndex={0}
                            onKeyDown={(e) => {
                              if (table.options.stopPropagation) e.stopPropagation();
                              table.options.onKeyDown?.(e);
                            }}
                          >
                            {row.cells.map((cell, index) => {
                              return (
                                <React.Fragment key={`${index}`}>
                                  {cell.isVisible && (
                                    <div
                                      className={`column col-${index}`}
                                      onClick={(e) => {
                                        if (table.options.stopPropagation) e.stopPropagation();
                                        table.options.onCellClick?.(cell, row, e);
                                      }}
                                    >
                                      {cell.cell(cell)}
                                    </div>
                                  )}
                                </React.Fragment>
                              );
                            })}
                          </div>
                        );
                      })}
                    </div>
                  </React.Fragment>
                );
              })}
            </div>
          )}
          {!table.groupBy &&
            table.page.map((row) => {
              return (
                <div
                  className={`row${row.isSelected ? ' selected' : ''}${
                    showActive && row.isActive ? ' active' : ''
                  }`}
                  key={_rowId(row.original)}
                  onClick={(e) => {
                    if (table.options.stopPropagation) e.stopPropagation();
                    table.options.onRowClick?.(row, e);
                  }}
                  tabIndex={0}
                  onKeyDown={(e) => {
                    if (table.options.stopPropagation) e.stopPropagation();
                    table.options.onKeyDown?.(e);
                  }}
                >
                  {row.cells
                    .filter((cell) => cell.isVisible)
                    .map((cell, index) => (
                      <div
                        className={`column col-${index}`}
                        key={`${index}`}
                        onClick={(e) => {
                          if (table.options.stopPropagation) e.stopPropagation();
                          table.options.onCellClick?.(cell, row, e);
                        }}
                      >
                        {cell.cell(cell)}
                      </div>
                    ))}
                </div>
              );
            })}
        </div>
        {table.options.showFooter && <footer className="footer"></footer>}
        {!table.groupBy && <TablePager table={table} />}
      </Container>
    </styled.FlexboxTable>
  );
};
