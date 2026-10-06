import React from 'react';
import { useApp, useReportInstances } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Loading, NotFound, Show } from 'tno-core';

import { ReportBody } from './ReportBody';
import * as styled from './styled';

export interface IReportInstanceViewProps {
  /** Report instance id. */
  instanceId: number;
  /** Regenerate report instance */
  regenerate?: boolean;
}

export const ReportInstanceView: React.FC<IReportInstanceViewProps> = ({
  instanceId,
  regenerate,
}) => {
  const [{ viewReportInstance }] = useReportInstances();
  const [{ requests }] = useApp();
  const [{ reportOutput }, { storeReportOutput }] = useProfileStore();
  const [notFoundId, setNotFoundId] = React.useState<number>();

  const isLoading = requests.some((r) => r.group.includes('view-report'));

  const handleViewReport = React.useCallback(
    async (instanceId: number) => {
      try {
        const response = await viewReportInstance(instanceId, regenerate);
        // The API answers 204 (no content) for a report instance that no longer exists.
        if (!response) setNotFoundId(instanceId);
        else storeReportOutput({ ...response, instanceId });
      } catch {}
    },
    [viewReportInstance, regenerate, storeReportOutput],
  );

  React.useEffect(() => {
    if (instanceId && reportOutput?.instanceId !== instanceId) {
      handleViewReport(instanceId);
    }
    // The functions will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instanceId, reportOutput]);

  if (notFoundId === instanceId) return <NotFound />;

  return (
    <styled.ReportInstanceView className="preview-section">
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Col className="preview-report">
        <div
          className="preview-subject"
          dangerouslySetInnerHTML={{ __html: reportOutput?.subject ?? '' }}
        ></div>
        <ReportBody html={reportOutput?.body ?? ''} />
      </Col>
    </styled.ReportInstanceView>
  );
};
