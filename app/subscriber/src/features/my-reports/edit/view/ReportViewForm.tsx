import { Button } from 'components/button';
import { Modal } from 'components/modal';
import { ReportStatus } from 'features/my-reports/ReportStatus';
import { calcNextReportSend, getLastSent } from 'features/my-reports/utils';
import React from 'react';
import { FaTelegramPlane } from 'react-icons/fa';
import { FaRegClock } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReportInstances, useReports } from 'store/hooks';
import {
  Claim,
  Col,
  IReportInstanceModel,
  ReportStatusName,
  Row,
  Show,
  Spinner,
  Text,
  useModal,
  validateEmail,
} from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

/**
 * ReportViewForm component.
 *
 * This component is responsible for rendering the report view form.
 * It uses the `useReportEditContext` hook to get the form values, submission status,
 * and functions to set field values and submission status.
 *
 * @returns A React component that renders the report view form.
 */
export const ReportViewForm: React.FC = () => {
  const { values, isSubmitting, setFieldValue, setSubmitting } = useReportEditContext();
  const [{ sendReportInstance }] = useReportInstances();
  const [, { getReportOwner }] = useReports();
  const [{ userInfo }] = useApp();

  const [to, setTo] = React.useState('');
  const { toggle: toggleSend, isShowing: isShowingSend } = useModal();
  const { toggle: toggleResend, isShowing: isShowingResend } = useModal();

  const instance = values.instances.length ? values.instances[0] : undefined;
  const isAdmin = userInfo?.roles.includes(Claim.administrator);
  const isSubscribed = values.subscribers.some((s) => s.isSubscribed === true);
  const [{ publishReportInstance }] = useReportInstances();

  const handleSend = React.useCallback(
    async (instanceId: number, to: string) => {
      try {
        await sendReportInstance(instanceId, to);
        toast.success('Report has been submitted.');
      } catch {}
    },
    [sendReportInstance],
  );

  const handleSendToOwner = React.useCallback(
    async (reportId: number, instanceId: number) => {
      try {
        const owner = await getReportOwner(reportId);
        const email = owner?.preferredEmail ? owner.preferredEmail : owner?.email;
        if (!email) {
          toast.error('This report does not have an owner');
        }
        await handleSend(instanceId, email!);
      } catch {}
    },
    [getReportOwner, handleSend],
  );

  const handlePublish = React.useCallback(
    async (instance: IReportInstanceModel, resend: boolean) => {
      try {
        setSubmitting(true);
        const updatedInstance = await publishReportInstance(instance.id, resend);
        setFieldValue(
          'instances',
          values.instances.map((i) =>
            i.id === instance.id ? { ...updatedInstance, content: instance?.content } : i,
          ),
        );
        toast.success(
          'Report has been submitted.  You will be notified when it is emailed to subscribers.',
        );
      } catch {
      } finally {
        setSubmitting(false);
      }
    },
    [publishReportInstance, setFieldValue, setSubmitting, values.instances],
  );

  return (
    <styled.ReportViewForm className="report-send report-edit-section">
      <Col>
        <Row>
          <Row alignItems="baseline" gap="1rem">
            <Col>
              <Row alignItems="baseline" gap="0.5rem">
                <FaRegClock />
                <div className="preview-block-headline">Status</div>
              </Row>
              <div className="preview-send-details-row">
                <Row gap="1rem">
                  <label className="b7">Status:</label>
                  <ReportStatus status={instance?.status} />
                </Row>
                <Row gap="1rem">
                  <label className="b7">Last sent:</label>
                  <span>{getLastSent(values)}</span>
                </Row>
                <Row gap="1rem">
                  <label className="b7">Next scheduled send:</label>
                  <span>{calcNextReportSend(values)}</span>
                </Row>
              </div>
            </Col>
          </Row>
          <Show visible={instance?.status === ReportStatusName.Submitted}>
            <Col>
              <Spinner />
            </Col>
          </Show>
        </Row>
      </Col>

      <Row gap="1rem">
        <Col flex="1">
          <Show visible={isAdmin || !!values.ownerId}>
            <Row alignItems="baseline" gap="0.5rem">
              <FaRegClock />
              <div className="preview-block-headline">Send a Test</div>
            </Row>
            <Row alignItems="flex-start" className="preview-send-details-row">
              <Button
                className="send-test-button"
                disabled={isSubmitting}
                variant="secondary"
                onClick={() => !!instance?.id && handleSendToOwner(values.id, instance.id)}
                style={{ backgroundColor: 'transparent' }}
              >
                <FaTelegramPlane />
                Send to Report Owner
              </Button>
            </Row>
          </Show>
          <Show visible={isAdmin}>
            <Row alignItems="flex-start" className="preview-send-details-row">
              <Col>
                <hr />
                <p>
                  Sending a report to a specific email address does not register the 'Last sent'
                  date. Only use this for testing.
                </p>
                <Text
                  label="Send to:"
                  name="email"
                  value={to}
                  onChange={(e) => setTo(e.target.value)}
                  width="300px"
                  type="email"
                >
                  <Button
                    className="send-test-button"
                    disabled={isSubmitting || !to.length || !validateEmail(to)}
                    variant="secondary"
                    onClick={() => !!instance?.id && handleSend(instance.id, to)}
                    style={{ backgroundColor: 'transparent' }}
                  >
                    <FaTelegramPlane />
                    Send test
                  </Button>
                </Text>
              </Col>
            </Row>
          </Show>
        </Col>
      </Row>

      <Row gap="1rem">
        <Col flex="1">
          <Row alignItems="baseline" gap="0.5rem">
            <FaTelegramPlane />
            <div className="preview-block-headline">Send Report</div>
          </Row>

          <Row alignItems="flex-start" className="preview-send-details-row">
            <Button
              disabled={
                isSubmitting ||
                !instance ||
                instance?.status === ReportStatusName.Submitted ||
                !isSubscribed
              }
              onClick={() => toggleSend()}
              variant="success"
            >
              Send to all subscribers
              <FaTelegramPlane />
            </Button>
            <Show
              visible={
                !!instance &&
                [ReportStatusName.Cancelled, ReportStatusName.Failed].includes(instance.status)
              }
            >
              <Button
                disabled={
                  isSubmitting ||
                  !instance ||
                  instance?.status === ReportStatusName.Submitted ||
                  !isSubscribed
                }
                onClick={() => toggleResend()}
                variant="warn"
              >
                Retry
                <FaTelegramPlane />
              </Button>
            </Show>
          </Row>
          {values.settings.doNotSendEmail && (
            <p className="info">You currently have the email preference to not send emails.</p>
          )}
        </Col>
      </Row>
      <Modal
        headerText="Send Report to Subscribers"
        body={`Do you want to send an email to the subscribers of this report? ${
          instance?.sentOn ? 'This report has already been sent out by email.' : ''
        }`}
        isShowing={isShowingSend}
        onClose={toggleSend}
        type="default"
        confirmText="Yes, send report to subscribers"
        onConfirm={async () => {
          try {
            if (instance) await handlePublish(instance, !!instance.sentOn);
          } finally {
            toggleSend();
          }
        }}
      />
      <Modal
        headerText="Retry Sending Report to Subscribers"
        body={`Do you want to send an email to the only the subscribers of this report that failed on the prior attempt?`}
        isShowing={isShowingResend}
        onClose={toggleResend}
        type="default"
        confirmText="Yes, send report to subscribers"
        onConfirm={async () => {
          try {
            if (instance) await handlePublish(instance, false);
          } finally {
            toggleResend();
          }
        }}
      />
    </styled.ReportViewForm>
  );
};
