import { TableBuilder, TableExporter } from 'components/export';
import React, { useEffect, useState } from 'react';
import { useUsers } from 'store/hooks';
import { UserAccountTypeName } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';

export const ReportSubscriberExporter: React.FC = () => {
  const { values } = useReportEditContext();
  const tblRef = React.useRef(null);
  const { getDistributionListById } = useUsers();
  const [rowData, setRowData] = useState<any[]>([]);

  const fetchEmails = React.useCallback(async () => {
    const allEmails = await Promise.all(
      values.subscribers.map(async (sub) => {
        if (sub.accountType === UserAccountTypeName.Distribution) {
          // Fetch emails related to a distribution list.
          const users = await getDistributionListById(sub.userId);
          return users.map((user: { username: string; email: string; displayName: string }) => ({
            username: user.username,
            email: user.email,
            format: sub.format,
            displayName: user.displayName,
          }));
        } else {
          // Regular subscriber
          return [
            {
              username: sub.username,
              email: sub.email,
              format: sub.format,
              displayName: sub.displayName,
            },
          ];
        }
      }),
    );

    const flattenedEmails = allEmails.flat();

    // Remove duplicates based on the 'email' field
    const uniqueEmails = flattenedEmails.reduce(
      (
        acc: Array<{ username: string; email: string; format: string; displayName: string }>,
        current,
      ) => {
        const x = acc.find((item) => item.email === current.email);
        if (!x) {
          acc.push(current);
        }
        return acc;
      },
      [],
    );

    return uniqueEmails;
  }, [getDistributionListById, values.subscribers]);

  useEffect(() => {
    const loadData = async () => {
      const emailData = await fetchEmails();
      setRowData(
        emailData.map((sub) => ({
          data: [sub.username, sub.email, sub.format, sub.displayName],
        })),
      );
    };

    loadData();
  }, [fetchEmails]);

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
        rowData={rowData}
      />
      <TableExporter
        label="Export subscriber list"
        ref={tblRef.current}
        filename="subscribers.xlsx"
      />
    </>
  );
};
