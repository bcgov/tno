import React from 'react';
import { useApp, useReportInstances } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Loading, Show } from 'tno-core';

export interface IReportInstanceViewProps {
  /** Report instance id. */
  instanceId: number;
}

export const ReportInstanceView: React.FC<IReportInstanceViewProps> = ({ instanceId }) => {
  const [{ viewReportInstance }] = useReportInstances();
  const [{ requests }] = useApp();
  const [{ reportOutput }, { storeReportOutput }] = useProfileStore();

  const isLoading = requests.some((r) => r.group.includes('view-report'));

  const handleViewReport = React.useCallback(
    async (instanceId: number) => {
      try {
        const response = await viewReportInstance(instanceId, true);
        storeReportOutput({ ...response, instanceId });
      } catch {}
    },
    [viewReportInstance, storeReportOutput],
  );

  React.useEffect(() => {
    if (instanceId && reportOutput?.instanceId !== instanceId) {
      handleViewReport(instanceId);
    }
    // The functions will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instanceId, reportOutput]);

  return (
    <div className="preview-section">
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Col className="preview-report">
        <div
          className="preview-subject"
          dangerouslySetInnerHTML={{ __html: reportOutput?.subject ?? '' }}
        ></div>
        <div
          className="preview-body"
          dangerouslySetInnerHTML={{ __html: reportOutput?.body ?? '' }}
        ></div>
      </Col>
    </div>
  );
};
