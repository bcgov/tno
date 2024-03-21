import { Button } from 'components/button';
import React from 'react';
import { FaFileExcel } from 'react-icons/fa6';
import { utils, writeFileXLSX } from 'sheetjs';

export interface ITableExporterProps {
  label: string;
  icon?: React.ReactNode;
  filename?: string;
}

export const TableExporter = React.forwardRef<HTMLTableElement, ITableExporterProps>(
  ({ label, icon = <FaFileExcel />, filename = 'download.xlsx' }, ref) => {
    const exportSubscribers = React.useCallback(() => {
      const wb = utils.table_to_book(ref);
      writeFileXLSX(wb, filename);
    }, [filename, ref]);

    return (
      <Button variant="secondary" onClick={() => exportSubscribers()}>
        {label}
        {icon}
      </Button>
    );
  },
);
