import React from 'react';
import { createRoot } from 'react-dom/client';
import { utils, writeFileXLSX } from 'sheetjs';

export const useTableExport = () => {
  const exportSubscribers = React.useCallback((ref: React.MutableRefObject<HTMLDivElement>) => {
    // const container = document.getElementById('export-excel');
    const root = createRoot(ref.current!);
    root.render(
      <table
        ref={(e) => {
          const wb = utils.table_to_book(e);
          writeFileXLSX(wb, 'subscribers.xlsx');
        }}
      >
        <thead>
          <tr>
            <td>h1</td>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>v1</td>
          </tr>
        </tbody>
      </table>,
    );
  }, []);

  return exportSubscribers;
};
