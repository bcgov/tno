import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import { IReportForm } from 'features/my-reports/interfaces';
import { calcNextReportSend, getLastSent } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaTelegramPlane, FaTrash } from 'react-icons/fa';
import { FaRegCalendarDays } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReportInstances } from 'store/hooks';
import {
  Claim,
  Col,
  getEnumStringOptions,
  IUserReportModel,
  Modal,
  OptionItem,
  ReportDistributionFormatName,
  ReportStatusName,
  Row,
  Select,
  Show,
  Spinner,
  Text,
  useApiAdminUsers,
  useModal,
  validateEmail,
} from 'tno-core';

import * as styled from './styled';

/**
 * Provides component to enable the user to send their report to subscribers.
 * @returns Component.
 */
export const ReportSendForm: React.FC = () => {
  const { values, isSubmitting, setFieldValue } = useFormikContext<IReportForm>();
  const [{ sendReportInstance, publishReportInstance }] = useReportInstances();
  const [{ userInfo }] = useApp();
  const { isShowing, toggle } = useModal();
  const { findUsers } = useApiAdminUsers();

  const [to, setTo] = React.useState('');
  const [email, setEmail] = React.useState('');

  const instance = values.instances.length ? values.instances[0] : undefined;
  const instanceId = instance?.id;
  const isAdmin = userInfo?.roles.includes(Claim.administrator);
  const formatOptions = getEnumStringOptions(ReportDistributionFormatName);

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

  const addSubscriber = React.useCallback(
    async (email: string) => {
      try {
        const response = await findUsers({ email });
        const users = response.data;
        if (users.items.length) {
          const subscribers: IUserReportModel[] = users.items
            .filter((user) => !values.subscribers.some((s) => s.userId === user.id))
            .map<IUserReportModel>((user) => ({
              ...user,
              userId: user.id,
              reportId: values.id,
              isSubscribed: true,
              format: ReportDistributionFormatName.FullText,
              version: 0,
            }));
          setFieldValue('subscribers', [...values.subscribers, ...subscribers]);
        } else {
          toast.warning(`No users found for the specified email "${email}".`);
        }
      } catch {}
    },
    [findUsers, setFieldValue, values.id, values.subscribers],
  );

  return (
    <styled.ReportSendForm className="report-send">
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
                <span>{calcNextReportSend(values)}</span>
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
          <Col>Format</Col>
          {isAdmin && <Col></Col>}
        </Row>
        {values.subscribers
          .filter((s) => s.isSubscribed)
          .map((sub, index) => {
            return (
              <Row key={sub.userId} className="row">
                <Col flex="1">{sub.username}</Col>
                <Col flex="1">{sub.lastName}</Col>
                <Col flex="1">{sub.firstName}</Col>
                <Col flex="2">{sub.email}</Col>
                <Col>
                  {!isAdmin ? (
                    sub.format
                  ) : (
                    <Select
                      name={`subscribers.${index}.format`}
                      options={formatOptions}
                      value={formatOptions.find((o) => o.value === sub.format) ?? ''}
                      onChange={(newValue) => {
                        const option = newValue as OptionItem;
                        if (option) {
                          setFieldValue(
                            'subscribers',
                            values.subscribers.map((s) =>
                              s.userId === sub.userId ? { ...sub, format: option.value } : s,
                            ),
                          );
                        }
                      }}
                      isClearable={false}
                    />
                  )}
                </Col>
                {isAdmin && (
                  <Col>
                    <Action
                      icon={<FaTrash />}
                      onClick={() =>
                        setFieldValue(
                          'subscribers',
                          values.subscribers.filter((s) => s.userId !== sub.userId),
                        )
                      }
                    />
                  </Col>
                )}
              </Row>
            );
          })}
        {isAdmin && (
          <Col>
            <h2>Add subscribers to report</h2>
            <Row alignItems="center">
              <Text
                name="email"
                label="Search by email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
              />
              <Button disabled={!validateEmail(email)} onClick={() => addSubscriber(email)}>
                Add subscriber
              </Button>
            </Row>
          </Col>
        )}
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
    </styled.ReportSendForm>
  );
};
