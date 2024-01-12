import { Button } from 'components/button';
import { Section } from 'components/section';
import { IReportForm } from 'features/my-reports/interfaces';
import { calcNextSend, getLastSent } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaTelegramPlane } from 'react-icons/fa';
import { FaRegCalendarDays } from 'react-icons/fa6';
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

export const ReportSendForm: React.FC = () => {
  const { values, isSubmitting, setFieldValue } = useFormikContext<IReportForm>();
  const [{ sendReportInstance, publishReportInstance }] = useReportInstances();
  const [{ userInfo }] = useApp();
  const { isShowing, toggle } = useModal();

  const [to, setTo] = React.useState('');

  const instance = values.instances.length ? values.instances[0] : undefined;
  const instanceId = instance?.id;
  const isAdmin = userInfo?.roles.includes(Claim.administrator);

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
        const updatedInstance = await publishReportInstance(id);
        setFieldValue(
          'instances',
          values.instances.map((i) =>
            i.id === id ? { ...updatedInstance, content: instance?.content } : i,
          ),
        );
        toast.success('Report has been submitted.');
      } catch {}
    },
    [instance?.content, publishReportInstance, setFieldValue, values.instances],
  );

  return (
    <Col className="report-send">
      {isAdmin && (
        <Col>
          <Section showOpen label="Test">
            <Row>
              <Col flex="1">
                <p>
                  Sending a report to a specific email address does not register the 'Last sent'
                  date. Only use this for testing.
                </p>
              </Col>
              <Col flex="2">
                <Row justifyContent="center" alignItems="center">
                  <Text
                    label="Send to:"
                    name="email"
                    value={to}
                    onChange={(e) => setTo(e.target.value)}
                    width="400px"
                    type="email"
                  />
                  <Button
                    disabled={isSubmitting || !to.length || !validateEmail(to)}
                    onClick={() => !!instanceId && handleSend(instanceId, to)}
                  >
                    Send email
                    <FaTelegramPlane />
                  </Button>
                </Row>
              </Col>
            </Row>
          </Section>
        </Col>
      )}
      <Row>
        {values.events.some((e) => e.isEnabled) && (
          <Row gap="1rem">
            <Col>
              <FaRegCalendarDays className="" />
            </Col>
            <Col>
              <Row gap="1rem" className="fs1">
                <label className="b7">Last sent:</label>
                <span>{getLastSent(values)}</span>
              </Row>
              <Row gap="1rem" className="fs1">
                <label className="b7">Next scheduled send:</label>
                <span>{calcNextSend(values)}</span>
              </Row>
            </Col>
          </Row>
        )}
        <Col flex="1" alignContent="center" justifyContent="center">
          <Button
            disabled={
              isSubmitting || !instanceId || instance?.status === ReportStatusName.Submitted
            }
            onClick={() => toggle()}
          >
            Send to subscribers
            <FaTelegramPlane />
          </Button>
        </Col>
        <Show visible={instance?.status === ReportStatusName.Submitted}>
          <Col>
            <Spinner />
          </Col>
        </Show>
      </Row>
      <Col className="subscribers">
        <Row className="header">
          <Col flex="1">Username</Col>
          <Col flex="1">LastName</Col>
          <Col flex="1">FirstName</Col>
          <Col flex="2">Email</Col>
          <Col flex="1">Format</Col>
        </Row>
        {values.subscribers
          .filter((s) => s.isSubscribed)
          .map((sub) => {
            return (
              <Row key={sub.id} className="row">
                <Col flex="1">{sub.username}</Col>
                <Col flex="1">{sub.lastName}</Col>
                <Col flex="1">{sub.firstName}</Col>
                <Col flex="2">{sub.email}</Col>
                <Col flex="1">{sub.format}</Col>
              </Row>
            );
          })}
      </Col>
      <Modal
        headerText="Send Report to Subscribers"
        body={`Do you want to send an email to the subscribers of this report? ${
          instance?.sentOn ? 'This report has already been sent out by email.' : ''
        }`}
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Yes, send report to subscribers"
        onConfirm={async () => {
          try {
            if (instanceId) await handlePublish(instanceId);
          } finally {
            toggle();
          }
        }}
      />
    </Col>
  );
};
