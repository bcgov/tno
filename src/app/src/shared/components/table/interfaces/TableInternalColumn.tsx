import { getProperty } from '../../../utils';
import { ITableInternalCell } from './ITableInternalCell';
import { ITableInternalColumn } from './ITableInternalColumn';

export class TableInternalColumn<T extends object> implements ITableInternalColumn<T> {
  index: number;
  accessor: keyof T | string | undefined | ((data: T) => unknown);
  label: React.ReactNode; // TODO: Separate header columns and row columns.
  cell: (cell: ITableInternalCell<T>) => React.ReactNode;
  isVisible: boolean;
  hAlign?: 'left' | 'center' | 'right';
  vAlign?: 'center' | 'top' | 'bottom';
  width?: string | number;

  constructor(
    index: number,
    assessor: keyof T | string | undefined | ((data: T) => unknown),
    label: React.ReactNode,
    cell: ((cell: ITableInternalCell<T>) => React.ReactNode) | undefined,
    options: {
      isVisible?: boolean;
      hAlign?: 'left' | 'center' | 'right';
      vAlign?: 'center' | 'top' | 'bottom';
      width?: string | number;
    } = {
      isVisible: true,
    },
  ) {
    this.index = index;
    this.accessor = assessor;
    this.label = label;
    this.cell = cell
      ? cell
      : (cell) => {
          return typeof assessor === 'function' ? (
            <>{assessor(cell.original)}</>
          ) : (
            <>{getProperty(cell.original, assessor)}</>
          );
        };
    this.isVisible = options.isVisible ?? true;
    this.hAlign = options?.hAlign;
    this.vAlign = options?.vAlign;
    this.width = options?.width;
  }
}
