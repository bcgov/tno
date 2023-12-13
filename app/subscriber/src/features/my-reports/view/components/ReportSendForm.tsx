import { Button } from 'components/button';
import { IReportForm } from 'features/my-reports/interfaces';
import { calcNextSend, getLastSent } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaTelegramPlane } from 'react-icons/fa';
import { FaRegCalendarDays } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReportInstances } from 'store/hooks';
import { Claim, Col, Modal, Row, Section, Text, useModal, validateEmail } from 'tno-core';

export const ReportSendForm: React.FC = () => {
  const { values, isSubmitting } = useFormikContext<IReportForm>();
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
        await publishReportInstance(id);
        toast.success('Report has been submitted.');
      } catch {}
    },
    [publishReportInstance],
  );

  return (
    <Col className="report-send">
      <Row>
        {values.events.some((e) => e.isEnabled) && (
          <Col>
            <div>
              <FaRegCalendarDays />
            </div>
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
          </Col>
        )}
        <Col flex="1" alignContent="center" justifyContent="center">
          <Button disabled={isSubmitting || !instanceId} onClick={() => toggle()}>
            Send to subscribers
            <FaTelegramPlane />
          </Button>
        </Col>
      </Row>
      {isAdmin && (
        <Col>
          <Section>
            <p>
              Sending a report to a specific email address does not register the 'Last sent' date.
              Only use this for testing.
            </p>
            <Row justifyContent="center">
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
          </Section>
        </Col>
      )}
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
