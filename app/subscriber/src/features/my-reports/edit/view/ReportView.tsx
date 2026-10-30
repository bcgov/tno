import React from 'react';
import { FaEye } from 'react-icons/fa6';
import { useParams } from 'react-router-dom';
import { useApiHub, useApp, useReportInstances } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  AISectionStatusName,
  Col,
  IReportMessageModel,
  MessageTargetKey,
  ReportPreviewStatus,
  Row,
} from 'tno-core';

import { ReportBody } from '../../ReportBody';
import { useReportEditContext } from '../ReportEditContext';
import { ReportViewPromptButton } from './ReportViewPromptButton';
import * as styled from './styled';

export const ReportView = () => {
  const { id } = useParams();
  const { values, previewLastUpdatedOn, setPreviewLastUpdatedOn } = useReportEditContext();
  const [{ requests }] = useApp();
  const [{ reportOutput }] = useProfileStore();
  const [{ viewReportInstance }] = useReportInstances();
  const hub = useApiHub();
  const instance = values.id === Number(id) ? values.instances[0] : undefined;
  const instanceId = instance?.id;
  const updatedOn = instance?.updatedOn;
  const isLoading = requests.some((r) => r.group.includes('view-report'));
  const output = instanceId && reportOutput?.instanceId === instanceId ? reportOutput : undefined;
  const aiSections = output?.aiSections;

  const handleViewReport = React.useCallback(
    async (instanceId: number, regenerate: boolean) => {
      if (!instanceId) return;

      try {
        await viewReportInstance(instanceId, regenerate);
      } catch {}
    },
    [viewReportInstance],
  );

  React.useEffect(() => {
    if (instanceId && (!output || updatedOn !== previewLastUpdatedOn)) {
      setPreviewLastUpdatedOn(updatedOn);
      handleViewReport(instanceId, !instance.sentOn);
    }
    // Initialize every time this component is displayed.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [instanceId, updatedOn, instance?.sentOn, reportOutput?.instanceId]);

  // AI sections are generated in the background; refresh the preview when they are ready.
  hub.useHubEffect(MessageTargetKey.ReportStatus, async (message: IReportMessageModel) => {
    if (message.message === 'ai-sections' && message.id === instanceId) {
      await handleViewReport(instanceId, !instance?.sentOn);
    }
  });

  // Completion notifications can be missed (or sent to another viewer). Recheck queued and
  // running sections until they reach a terminal state, including when a claim expires.
  const isWaitingForAI = aiSections?.some((section) =>
    [AISectionStatusName.NotStarted, AISectionStatusName.Generating].includes(section.status),
  );
  React.useEffect(() => {
    if (!isWaitingForAI || isLoading || !instanceId) return;
    const timer = setTimeout(() => handleViewReport(instanceId, !instance?.sentOn), 30000);
    return () => clearTimeout(timer);
  }, [isWaitingForAI, isLoading, instanceId, instance?.sentOn, handleViewReport, aiSections]);

  if (output?.sizeError)
    return (
      <div role="alert">
        <strong>Report too large</strong>
        <p>{output.sizeError}</p>
      </div>
    );

  return (
    <styled.ReportView className="report-edit-section">
      <div>
        <Row className="report-edit-headline-row" alignItems="first baseline" gap="0.5em">
          <FaEye size={18} />
          <h1>Preview Report</h1>
          <div>
            <ReportViewPromptButton instanceId={instanceId} />
          </div>
        </Row>
      </div>
      <ReportPreviewStatus isRequesting={isLoading} sections={aiSections} />
      <Col className="preview-report">
        <div
          className="preview-subject"
          dangerouslySetInnerHTML={{ __html: output?.subject ?? '' }}
        ></div>
        <ReportBody html={output?.body ?? ''} />
      </Col>
    </styled.ReportView>
  );
};
