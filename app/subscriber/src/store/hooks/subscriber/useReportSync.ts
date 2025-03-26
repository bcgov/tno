import React from 'react';
import { useProfileStore } from 'store/slices';
import { IReportMessageModel, MessageTargetKey, useApiSubscriberReportInstances } from 'tno-core';

import { useApiHub } from '../signalr';
import { useReports } from './useReports';

/**
 * Hook provides a singleton way to ensure my reports are synced across tabs.
 */
export const useReportSync = () => {
  const { getReportInstance } = useApiSubscriberReportInstances();
  const [, { getReport }] = useReports();
  const hub = useApiHub();
  const [{ myReports }, { storeMyReports }] = useProfileStore();

  // When a report instance has been updated a message will be received.
  const handleUpdateReportInstance = React.useCallback(
    async (message: IReportMessageModel) => {
      try {
        const report = myReports.find((r) => r.id === message.reportId);
        const instance = report?.instances.length ? report.instances[0] : undefined;

        // Only fetch the latest if the current report instance is older, or doesn't exist.
        if (report && (!instance || instance.version !== message.version)) {
          if (message.message === 'status') {
            // Update the status of the instance.
            storeMyReports((reports) => {
              const results = reports.map((r) =>
                r.id === report.id
                  ? {
                      ...report,
                      instances: report.instances.map((i) =>
                        i.id === message.id
                          ? { ...i, status: message.status, version: message.version }
                          : i,
                      ),
                    }
                  : r,
              );
              return results;
            });
          } else if (message.message === 'event') {
            const updateReport = await getReport(report.id, false);
            if (updateReport) {
              storeMyReports((reports) => {
                const results = reports.map((r) =>
                  r.id === report.id
                    ? {
                        ...report,
                        events: updateReport.events,
                        version: updateReport.version,
                      }
                    : r,
                );
                return results;
              });
            }
          } else {
            const response = await getReportInstance(message.id, true);
            if (response.status === 200 && response.data) {
              const instance = response.data;
              storeMyReports((reports) => {
                let addInstance = true;
                let instances = report.instances.map((i) => {
                  if (i.id === message.id) {
                    addInstance = false;
                    return instance;
                  }
                  return i;
                });
                if (addInstance) instances = [instance, ...instances];
                const results = reports.map((r) =>
                  r.id === report.id
                    ? {
                        ...report,
                        instances,
                      }
                    : r,
                );
                return results;
              });
            }
          }
        }
      } catch {}
    },
    [getReportInstance, myReports, storeMyReports, getReport],
  );

  hub.useHubEffect(MessageTargetKey.ReportStatus, handleUpdateReportInstance);
};
