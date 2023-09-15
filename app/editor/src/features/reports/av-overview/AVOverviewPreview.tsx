import React from 'react';
import { FaPaperPlane } from 'react-icons/fa';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAVOverviewInstances } from 'store/hooks';
import {
  Button,
  Col,
  IAVOverviewInstanceModel,
  IReportResultModel,
  Loading,
  Modal,
  Row,
  Show,
  useModal,
} from 'tno-core';

import * as styled from './styled';

const AVOverviewPreview: React.FC = () => {
  const [{ getAVOverview, previewAVOverview, publishAVOverview }] = useAVOverviewInstances();
  const { toggle, isShowing } = useModal();
  const { id } = useParams();
  const instanceId = parseInt(id ?? '');

  const [isLoading, setIsLoading] = React.useState(true);
  const [instance, setInstance] = React.useState<IAVOverviewInstanceModel>();
  const [preview, setPreview] = React.useState<IReportResultModel | undefined>();

  const handlePreviewReport = React.useCallback(
    async (instanceId: number) => {
      try {
        setIsLoading(true);
        const instance = await getAVOverview(instanceId);
        setInstance(instance);
        const preview = await previewAVOverview(instanceId);
        setPreview(preview);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [getAVOverview, previewAVOverview],
  );

  React.useEffect(() => {
    handlePreviewReport(instanceId);
  }, [handlePreviewReport, instanceId]);

  const handlePublish = React.useCallback(async () => {
    try {
      await publishAVOverview(instanceId);
      toast.success('Evening Overview report request to publish has been sent.');
    } catch {}
  }, [publishAVOverview, instanceId]);

  return (
    <styled.AVOverviewPreview>
      <Row className="page-header">
        <h1>Evening Overview</h1>
        <div className="buttons">
          <Button disabled={!instanceId} onClick={() => toggle()}>
            Publish <FaPaperPlane className="icon" />
          </Button>
        </div>
      </Row>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading}>
        <Col className="preview-report">
          <div
            className="preview-subject"
            dangerouslySetInnerHTML={{ __html: preview?.subject ?? '' }}
          ></div>
          <div
            className="preview-body"
            dangerouslySetInnerHTML={{ __html: preview?.body ?? '' }}
          ></div>
        </Col>
      </Show>
      <Modal
        headerText="Confirm Publish"
        body={
          instance?.isPublished
            ? 'This evening overview has already been published. Are you sure you wish to publish again?'
            : 'Are you sure you wish to publish the evening overview?'
        }
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, Publish"
        onConfirm={async () => {
          try {
            await handlePublish();
          } catch {
            // Globally handled
          } finally {
            toggle();
          }
        }}
      />
    </styled.AVOverviewPreview>
  );
};

export default AVOverviewPreview;
