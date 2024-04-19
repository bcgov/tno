import { TableBuilder, TableExporter } from 'components/export';
import React from 'react';

import { useReportEditContext } from '../ReportEditContext';

export const ReportSubscriberExporter = () => {
  const { values } = useReportEditContext();
  const tblRef = React.useRef(null);

  return (
    <>
      <TableBuilder
        ref={tblRef}
        isHidden
        columns={[
          { label: 'username' },
          { label: 'email' },
          { label: 'format' },
          { label: 'name' },
        ]}
        rowData={values.subscribers.map((sub) => ({
          data: [sub.username, sub.email, sub.format, sub.displayName],
        }))}
      />
      <TableExporter
        label="Export subscriber list"
        ref={tblRef.current}
        filename="subscribers.xlsx"
      />
    </>
  );
};
