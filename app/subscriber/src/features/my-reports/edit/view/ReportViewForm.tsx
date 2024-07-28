import { Button } from 'components/button';
import { calcNextReportSend, getLastSent } from 'features/my-reports/utils';
import React from 'react';
import { FaTelegramPlane } from 'react-icons/fa';
import { FaRegClock } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReportInstances } from 'store/hooks';
import {
  Claim,
  Col,
  Modal,
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
  const [{ userInfo }] = useApp();

  const [to, setTo] = React.useState('');
  const { toggle: toggleSend, isShowing: isShowingSend } = useModal();

  const instance = values.instances.length ? values.instances[0] : undefined;
  const isAdmin = userInfo?.roles.includes(Claim.administrator);
  const [{ publishReportInstance }] = useReportInstances();

  const handleSend = React.useCallback(
    async (id: number, to: string) => {
      try {
        await sendReportInstance(id, to);
        toast.success('Report has been submitted.');
      } catch {}
    },
    [sendReportInstance],
  );

  const handlePublish = React.useCallback(
    async (id: number) => {
      try {
        setSubmitting(true);
        const updatedInstance = await publishReportInstance(id);
        setFieldValue(
          'instances',
          values.instances.map((i) =>
            i.id === id ? { ...updatedInstance, content: instance?.content } : i,
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
    [instance?.content, publishReportInstance, setFieldValue, setSubmitting, values.instances],
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

      <Show visible={isAdmin}>
        <Row gap="1rem">
          <Col flex="1">
            <Row alignItems="baseline" gap="0.5rem">
              <FaRegClock />
              <div className="preview-block-headline">Send a Test</div>
            </Row>

            <Row alignItems="flex-start" className="preview-send-details-row">
              <Col>
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
          </Col>
        </Row>
      </Show>

      <Row gap="1rem">
        <Col flex="1">
          <Row alignItems="baseline" gap="0.5rem">
            <FaTelegramPlane />
            <div className="preview-block-headline">Send Report</div>
          </Row>

          <Row alignItems="flex-start" className="preview-send-details-row">
            <Button
              disabled={
                isSubmitting || !instance || instance?.status === ReportStatusName.Submitted
              }
              onClick={() => toggleSend()}
              variant="success"
            >
              Send to subscribers
              <FaTelegramPlane />
            </Button>
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
        hide={toggleSend}
        type="default"
        confirmText="Yes, send report to subscribers"
        onConfirm={async () => {
          try {
            if (instance) await handlePublish(instance.id);
          } finally {
            toggleSend();
          }
        }}
      />
    </styled.ReportViewForm>
  );
};
