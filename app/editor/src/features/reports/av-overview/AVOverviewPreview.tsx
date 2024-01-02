import moment from 'moment';
import React from 'react';
import { FaExternalLinkAlt, FaPaperPlane } from 'react-icons/fa';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAVOverviewInstances, useLookup } from 'store/hooks';
import {
  Button,
  Col,
  IAVOverviewInstanceModel,
  IReportResultModel,
  Loading,
  Modal,
  Row,
  Settings,
  Show,
  useModal,
} from 'tno-core';

import * as styled from './styled';

const AVOverviewPreview: React.FC = () => {
  const [{ settings }] = useLookup();
  const subscriberUrl = settings.find((i) => i.name === Settings.SubscriberUrl)?.value;
  const [{ getAVOverview, viewAVOverview, publishAVOverview }] = useAVOverviewInstances();
  const { toggle, isShowing } = useModal();
  const { id } = useParams();
  const instanceId = parseInt(id ?? '');

  const [isLoading, setIsLoading] = React.useState(true);
  const [instance, setInstance] = React.useState<IAVOverviewInstanceModel>();
  const [view, setView] = React.useState<IReportResultModel | undefined>();

  const handlePreviewReport = React.useCallback(
    async (instanceId: number) => {
      try {
        setIsLoading(true);
        const instance = await getAVOverview(instanceId);
        setInstance(instance);
        const view = await viewAVOverview(instanceId);
        setView(view);
      } catch {
      } finally {
        setIsLoading(false);
      }
    },
    [getAVOverview, viewAVOverview],
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
          <Show visible={instance?.isPublished}>
            <div
              className="view"
              onClick={() => {
                if (!subscriberUrl) {
                  toast.error('SubscriberUrl setting not found. Please set and try again.');
                  return;
                }
                window.open(
                  `${subscriberUrl}landing/eveningoverview?date=${moment(instance?.publishedOn)
                    .add(1, 'days')
                    .startOf('day')
                    .format('YYYY/MM/DD')}`,
                );
              }}
            >
              <div className="label-container">
                <FaExternalLinkAlt size={25} />
                <span>View on site</span>
              </div>
            </div>
          </Show>
        </div>
      </Row>
      <Show visible={isLoading}>
        <Loading />
      </Show>
      <Show visible={!isLoading}>
        <Col className="preview-report">
          <div
            className="preview-subject"
            dangerouslySetInnerHTML={{ __html: view?.subject ?? '' }}
          ></div>
          <div
            className="preview-body"
            dangerouslySetInnerHTML={{ __html: view?.body ?? '' }}
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
