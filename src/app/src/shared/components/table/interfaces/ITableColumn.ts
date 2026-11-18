export interface ITableColumn {
  isVisible?: boolean;
  showSort?: boolean;
  isSorted?: boolean;
  isSortedDesc?: boolean;
  hAlign?: 'left' | 'center' | 'right';
  vAlign?: 'top' | 'center' | 'bottom';
  width?: string | number;
}
