import React from 'react';

import * as styled from './styled';

export interface ITableBuilderColumn {
  key?: string;
  label: React.ReactNode;
}

export interface ITableBuilderRow {
  key?: string;
  data: React.ReactNode[];
}

export interface ITableBuilderProps extends React.AllHTMLAttributes<HTMLTableElement> {
  columns?: ITableBuilderColumn[];
  rowData?: ITableBuilderRow[];
  showHeader?: boolean;
  isHidden?: boolean;
}

export const TableBuilder = React.forwardRef<HTMLTableElement, ITableBuilderProps>(
  ({ columns, rowData, className, showHeader = true, isHidden, ...rest }, ref) => {
    return (
      <styled.TableBuilder
        ref={ref}
        className={`export-table${className ? ` ${className}` : ''}`}
        showHeader={showHeader}
        isHidden={isHidden}
      >
        <thead>
          <tr>
            {columns?.map((column, index) => (
              <td key={column.key ?? index}>{column.label}</td>
            ))}
          </tr>
        </thead>
        <tbody>
          {rowData?.map((row, index) => (
            <tr key={row.key ?? index}>
              {row.data.map((column, index) => (
                <td key={index}>{column}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </styled.TableBuilder>
    );
  },
);
