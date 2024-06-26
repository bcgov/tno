import { Action } from 'components/action/Action';
import React from 'react';
import { FaEye, FaX } from 'react-icons/fa6';
import { useApp, useReportInstances } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Loading, Row, Show } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

export const ReportView = () => {
  const { values } = useReportEditContext();
  const [{ requests }] = useApp();
  const [{ reportOutput }, { storeReportOutput }] = useProfileStore();
  const [{ viewReportInstance }] = useReportInstances();

  const instanceId = values.instances.length ? values.instances[0].id : undefined;
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
    if (instanceId) {
      handleViewReport(instanceId);
    }
    // Initialize every time this component is displayed.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  if (!reportOutput) return null;

  return (
    <styled.ReportView className="report-edit-section">
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <div>
        <Row className="report-edit-headline-row" alignItems="first baseline" gap="0.5em">
          <FaEye size={18} />
          <h1>Preview Report</h1>
          <Action
            icon={<FaX className="icon-close" />}
            onClick={() => storeReportOutput(undefined)}
            id="icon-close"
          />
        </Row>
      </div>

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
    </styled.ReportView>
  );
};
