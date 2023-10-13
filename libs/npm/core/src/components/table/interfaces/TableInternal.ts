import React from 'react';

import { groupBy } from '../utils/groupBy';
import { initColumns } from '../utils/initColumns';
import { sortRows } from '../utils/sortRows';
import { IdType } from './IdType';
import { ITableGroup } from './ITableGroup';
import { ITableHookColumn } from './ITableHookColumn';
import { ITableHookFilter } from './ITableHookFilter';
import { ITableHookGrouping } from './ITableHookGrouping';
import { ITableHookOptions } from './ITableHookOptions';
import { ITableHookPaging } from './ITableHookPaging';
import { ITableHookSorting } from './ITableHookSorting';
import { ITableInternal } from './ITableInternal';
import { ITableInternalColumn } from './ITableInternalColumn';
import { ITableInternalFooter } from './ITableInternalFooter';
import { ITableInternalHeader } from './ITableInternalHeader';
import { ITableInternalOptions } from './ITableInternalOptions';
import { ITableInternalRow } from './ITableInternalRow';
import { ITableInternalRowGroup } from './ITableInternalRowGroup';
import { ITablePage } from './ITablePage';
import { ITableSort } from './ITableSort';
import { TableInternalColumn } from './TableInternalColumn';
import { TableInternalHeaderColumn } from './TableInternalHeaderColumn';
import { TableInternalRow } from './TableInternalRow';

export class TableInternal<T extends object> implements ITableInternal<T> {
  rowId: keyof T | ((data?: T) => string);
  _rowId: (data?: T) => string;
  columns: ITableInternalColumn<T>[];
  data: T[];
  rows: ITableInternalRow<T>[];
  header: ITableInternalHeader<T>;
  footer: ITableInternalFooter<T>;
  options: ITableInternalOptions<T>;
  activeRowId?: IdType<T>;
  activeRow: ITableInternalRow<T> | null;
  selectedRowIds: IdType<T>[];

  onSelectedChanged: (
    row: ITableInternalRow<T>,
    event: React.ChangeEvent<HTMLInputElement>,
  ) => void;

  /** Searches for the value in every column */
  filterData = (search: string) => {
    if (typeof search === 'string' && search.length) {
      return this.data.filter((item) => {
        return this.columns.some((col) => {
          if (item !== undefined) {
            const value =
              typeof col.accessor === 'function'
                ? col.accessor(item)
                : item[col.accessor as keyof T];
            return value ? `${value}`.includes(search) : false;
          }
          return false;
        });
      });
    }
    return this.data;
  };

  refreshPage = () => {
    const index = this.manualPaging ? 0 : this.pageIndex * this.pageSize;
    this.page = this.rows.slice(index, index + this.pageSize);
  };

  refreshRows = (selectedRows: ITableInternalRow<T>[] = []) => {
    const rows = this.filterData(this.search).map((original, index) => {
      const selected = this.selectedRowIds.some((id) => this._rowId(original) === id)
        ? true
        : selectedRows.find((row) => this._rowId(row.original) === this._rowId(original))
            ?.isSelected ?? false;
      return new TableInternalRow(this, index, this.rowId, this.columns, original, selected);
    });
    this.rows = sortRows(rows, this.sortOrder);

    if (this.groupBy) {
      if (typeof this.groupBy === 'function') {
        this.groups = groupBy(this.rows, this.groupBy).map((group) => ({
          key: group.key,
          rows: group.rows,
          heading: this.groupHeading,
        }));
      } else if (typeof this.groupBy === 'string') {
        this.groups = groupBy(this.rows, (item, index, array) => {
          const value = (item.original as any)[this.groupBy] ?? '';
          return value;
        }).map((group) => ({
          key: group.key,
          rows: group.rows,
          heading: this.groupHeading,
        }));
      }
    }
    this.refreshPage();
  };

  setActiveRow = (rowOrIndex?: null | number | ITableInternalRow<T>) => {
    // Remove the prior active row.
    if (this.activeRow !== null) {
      if (
        (rowOrIndex === undefined && rowOrIndex === null) ||
        (typeof rowOrIndex === 'number' && rowOrIndex !== this.activeRow.index) ||
        (typeof rowOrIndex !== 'number' && rowOrIndex?.index !== this.activeRow.index)
      ) {
        const index = this.rows.findIndex(
          (row) => this._rowId(row.original) === this._rowId(this.activeRow?.original),
        );
        if (index >= 0) {
          this.activeRowId = this._rowId(this.rows[index].original);
          this.rows[index].isActive = false;
        }
      }
    }

    if (rowOrIndex === undefined || rowOrIndex === null) {
      this.activeRow = null;
    } else if (typeof rowOrIndex === 'number') {
      this.rows[rowOrIndex].isActive = true;
      this.activeRow = this.rows[rowOrIndex];
      this.activeRowId = this._rowId(this.rows[rowOrIndex].original);
    } else {
      const index = this.rows.findIndex(
        (row) => this._rowId(row.original) === this._rowId(rowOrIndex.original),
      );
      if (index >= 0) {
        this.rows[index].isActive = true;
        this.activeRow = this.rows[index];
        this.activeRowId = this._rowId(this.rows[index].original);
      } else this.activeRow = null;
    }
  };

  toggleSelectedRow = (row: ITableInternalRow<T>) => {
    row.isSelected = !row.isSelected;
    return this.rows.filter((row) => row.isSelected);
  };

  // Paging
  page: ITableInternalRow<T>[];
  pagingEnabled: boolean;
  pageIndex: number;
  pageSize: number;
  pageNumber = () => this.pageIndex + 1;
  pageCount: number;
  totalItems?: number;
  calcPageCount = () =>
    this.data.length < this.pageSize ? 1 : Math.ceil(this.data.length / this.pageSize);
  manualPaging: boolean;
  pageButtons: number;
  showPaging: boolean;
  scrollSize: number | string;
  setPageSize = (pageSize: number) => {
    if (pageSize > 0) {
      this.pageIndex = 0;
      this.pageSize = pageSize;
      this.pageCount = this.data.length < pageSize ? 1 : Math.ceil(this.page.length / pageSize);
      this.onPageChange({ pageIndex: this.pageIndex, pageSize }, this);
    }
  };
  onPageChange: (page: ITablePage, table: ITableInternal<T>) => void;
  canPrevious = () => this.pageIndex > 0;
  canNext = () => this.pageIndex < this.pageCount - 1;
  goFirst = () => {
    this.pageIndex = 0;
    this.onPageChange({ pageIndex: 0, pageSize: this.pageSize }, this);
    return 0;
  };
  goPrevious = () => {
    const pageIndex = this.pageIndex - 1;
    if (this.canPrevious()) {
      this.pageIndex = pageIndex;
      this.onPageChange({ pageIndex, pageSize: this.pageSize }, this);
    }
    return pageIndex;
  };
  goNext = () => {
    const pageIndex = this.pageIndex + 1;
    if (this.canNext()) {
      this.pageIndex = pageIndex;
      this.onPageChange({ pageIndex, pageSize: this.pageSize }, this);
    }
    return pageIndex;
  };
  goLast = () => {
    const pageIndex = this.pageCount - 1;
    if (this.data.length) {
      this.pageIndex = pageIndex;
      this.onPageChange({ pageIndex, pageSize: this.pageSize }, this);
    }
    return pageIndex;
  };
  goToPageIndex = (pageIndex: number) => {
    if (this.pageCount > pageIndex) {
      this.pageIndex = pageIndex;
      this.onPageChange({ pageIndex, pageSize: this.pageSize }, this);
      return true;
    }
    return false;
  };

  // Sorting
  showSort: boolean;
  sortOrder: ITableSort<T>[];
  setSortOrder = (sortOrder: ITableSort<T>[]) => {
    this.sortOrder = sortOrder;
    this.onSortChange(sortOrder, this);
  };
  onSortChange: (sortOrder: ITableSort<T>[], table: ITableInternal<T>) => void;

  // Filtering
  showFilter: boolean;
  search: string;
  onFilterChange: (search: string, table: ITableInternal<T>) => void;
  applyFilter = (search: string) => {
    this.pageIndex = 0;
    this.search = search;
    this.refreshRows(this.rows);
    this.onFilterChange?.(search, this);
  };

  // Grouping
  groupBy?:
    | keyof T
    | ((item: ITableInternalRow<T>, index: number, array: ITableInternalRow<T>[]) => string);
  groupHeading: (group: ITableGroup<ITableInternalRow<T>>) => React.ReactNode;
  groups: ITableInternalRowGroup<ITableInternalRow<T>>[] = [];

  /**
   * Factory function to create a new table instance.
   * @param rowId The primary key of a row.
   * @param columns
   * @param data
   * @param options
   * @param paging
   * @param sorting
   * @param filter
   * @param grouping
   * @param rowState
   * @returns A new instance of a table.
   */
  static create<T extends object>(
    rowId: keyof T | ((data?: T) => string),
    columns: ITableHookColumn<T>[],
    data: T[],
    options: ITableHookOptions<T>,
    paging: ITableHookPaging<T>,
    sorting: ITableHookSorting<T>,
    filter: ITableHookFilter<T>,
    grouping: ITableHookGrouping<T> = {},
    rowState: ITableInternalRow<T>[] = [],
  ) {
    const table = new TableInternal<T>();
    table.rowId = rowId;
    table._rowId = typeof rowId === 'function' ? rowId : (data?: T) => `${data?.[rowId]}`;
    table.data = data;

    table.options = {
      ...options,
      showHeader: options.showHeader === undefined ? true : options.showHeader,
      showFooter: options.showFooter === undefined ? true : options.showFooter,
    };

    table.selectedRowIds = options.selectedRowIds ?? [];
    const activeRow = rowState.find((row) => row.isActive);
    // If an activeRowId is provided it will override any rowState provided.
    table.activeRowId =
      options.activeRowId ??
      (activeRow ? `${(activeRow.original as any)[table.rowId]}` : undefined);
    table.activeRow = table.activeRowId ? null : activeRow ?? null;
    table.onSelectedChanged = options.onSelectedChanged ? options.onSelectedChanged : () => {};

    // Paging
    table.pagingEnabled = paging.pagingEnabled ?? true;
    table.manualPaging = paging.manualPaging ?? false;
    table.pageIndex = paging.pageIndex ?? 0;
    table.pageSize = table.pagingEnabled ? paging.pageSize ?? 10 : table.data.length;
    table.totalItems = paging.totalItems;
    table.pageButtons = paging.pageButtons ?? 5;
    table.showPaging = table.pagingEnabled ? paging.showPaging ?? true : false;
    table.scrollSize = paging.scrollSize ?? 0;
    table.onPageChange = paging.onPageChange ? paging.onPageChange : () => {};

    // Sorting
    table.showSort = sorting.showSort ?? false;
    table.sortOrder = sorting.sortOrder ?? [];
    table.onSortChange = sorting.onSortChange ? sorting.onSortChange : () => {};

    // Filtering
    table.showFilter = filter.showFilter ?? false;
    table.search = filter.search ?? '';
    table.onFilterChange = filter.onFilterChange ? filter.onFilterChange : () => {};

    // Grouping
    table.groupBy = grouping.groupBy;
    table.groupHeading = grouping.groupHeading ? grouping.groupHeading : (group) => group.key;

    const headerColumns = initColumns(table, columns, sorting.sortOrder);
    table.header = {
      columns: headerColumns.map(
        (col, index) =>
          new TableInternalHeaderColumn(
            table,
            index,
            col.accessor,
            col.label,
            col.isVisible ?? true,
            col.sort,
            col.showSort,
            col.isSorted,
            col.isSortedDesc,
            {
              hAlign: col.hAlign,
              vAlign: col.vAlign,
              width: col.width,
            },
          ),
      ),
    };
    table.columns = headerColumns.map(
      (col, index) =>
        new TableInternalColumn(index, col.accessor, col.label, col.cell, {
          isVisible: col.isVisible,
          hAlign: col.hAlign,
          vAlign: col.vAlign,
          width: col.width,
        }),
    );
    table.footer = {
      columns: [],
    };

    table.refreshRows(rowState);
    table.pageCount = table.manualPaging
      ? paging.pageCount ?? table.calcPageCount()
      : table.calcPageCount();
    return table;
  }

  /**
   * Use the static factory method instead.
   */
  protected constructor() {
    this.rowId = '' as keyof T;
    this._rowId =
      typeof this.rowId === 'function'
        ? this.rowId
        : (data?: T) => `${data?.[this.rowId as keyof T]}`;
    this.header = {
      columns: [],
    };
    this.columns = [];
    this.footer = {
      columns: [],
    };
    this.data = [];
    this.rows = [];
    this.options = {};

    this.activeRow = null;
    this.selectedRowIds = [];
    this.onSelectedChanged = () => {};

    // Paging
    this.page = [];
    this.pagingEnabled = true;
    this.pageIndex = 0;
    this.pageSize = 0;
    this.pageCount = 0;
    this.manualPaging = false;
    this.pageButtons = 0;
    this.showPaging = false;
    this.scrollSize = 0;
    this.onPageChange = () => {};

    // Sorting
    this.showSort = false;
    this.sortOrder = [];
    this.onSortChange = () => {};

    // Filtering
    this.showFilter = false;
    this.search = '';
    this.onFilterChange = () => {};

    // Grouping
    this.groupHeading = () => undefined;
    this.groups = [];
  }
}
