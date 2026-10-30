import React from 'react';
import { useApp, useReportInstances } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Loading, NotFound, Show } from 'tno-core';

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
  const [{ reportOutput }] = useProfileStore();
  const [notFoundId, setNotFoundId] = React.useState<number>();

  const output = reportOutput?.instanceId === instanceId ? reportOutput : undefined;
  const isLoading = requests.some((r) => r.group.includes('view-report'));

  const handleViewReport = React.useCallback(
    async (instanceId: number) => {
      try {
        const response = await viewReportInstance(instanceId, regenerate);
        // The API answers 204 (no content) for a report instance that no longer exists.
        if (!response) setNotFoundId(instanceId);
      } catch {}
    },
    [viewReportInstance, regenerate],
  );

  React.useEffect(() => {
    if (instanceId && notFoundId !== instanceId && reportOutput?.instanceId !== instanceId) {
      handleViewReport(instanceId);
    }
    // The functions will result in infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instanceId, notFoundId, reportOutput?.instanceId]);

  if (notFoundId === instanceId) return <NotFound />;
  if (output?.sizeError)
    return (
      <div role="alert">
        <strong>Report too large</strong>
        <p>{output.sizeError}</p>
      </div>
    );

  return (
    <styled.ReportInstanceView className="preview-section">
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Col className="preview-report">
        <div
          className="preview-subject"
          dangerouslySetInnerHTML={{ __html: output?.subject ?? '' }}
        ></div>
        {/* Rendered without ReportBody's deferred rendering, which leaves WebKit (all iOS browsers)
            unable to scroll to the report's story anchors. */}
        <div
          className="preview-body"
          dangerouslySetInnerHTML={{ __html: output?.body ?? '' }}
        ></div>
      </Col>
    </styled.ReportInstanceView>
  );
};
