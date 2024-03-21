import { TableBuilder, TableExporter } from 'components/export';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';

export const ReportSubscriberExporter = () => {
  const { values } = useFormikContext<IReportForm>();
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
