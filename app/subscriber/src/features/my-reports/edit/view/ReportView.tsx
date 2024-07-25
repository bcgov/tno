import React from 'react';
import { FaEye } from 'react-icons/fa6';
import { useApp, useReportInstances } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Col, Loading, Overlay, Row, Show } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

export const ReportView = () => {
  const { values, previewLastUpdatedOn, setPreviewLastUpdatedOn } = useReportEditContext();
  const [{ requests }] = useApp();
  const [{ reportOutput }, { storeReportOutput }] = useProfileStore();
  const [{ viewReportInstance }] = useReportInstances();
  const instance = values.instances.length ? values.instances[0] : undefined;
  const instanceId = instance?.id;
  const updatedOn = instance?.updatedOn;
  const isLoading = requests.some((r) => r.group.includes('view-report'));

  const handleViewReport = React.useCallback(
    async (instanceId: any, regenerate: boolean) => {
      if (!instanceId) return;

      try {
        const response = await viewReportInstance(instanceId, regenerate);
        storeReportOutput({ ...response, instanceId });
      } catch {}
    },
    [viewReportInstance, storeReportOutput],
  );

  React.useEffect(() => {
    if (instanceId && updatedOn !== previewLastUpdatedOn) {
      setPreviewLastUpdatedOn(updatedOn);
      handleViewReport(instanceId, !instance.sentOn);
    }
    // Initialize every time this component is displayed.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instanceId, updatedOn, instance?.sentOn]);

  return (
    <styled.ReportView className="report-edit-section">
      <div>
        <Row className="report-edit-headline-row" alignItems="first baseline" gap="0.5em">
          <FaEye size={18} />
          <h1>Preview Report</h1>
          <div></div>
        </Row>
      </div>
      <Col className="preview-report">
        <Show visible={isLoading}>
          <Overlay>
            <Loading />
          </Overlay>
        </Show>
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
