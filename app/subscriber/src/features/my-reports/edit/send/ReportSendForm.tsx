import { Action } from 'components/action';
import { Button } from 'components/button';
import { Section } from 'components/section';
import { calcNextReportSend, getLastSent } from 'features/my-reports/utils';
import React from 'react';
import { FaTelegramPlane, FaTrash } from 'react-icons/fa';
import { FaRegCalendarDays } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useApp, useReportInstances } from 'store/hooks';
import {
  Claim,
  Col,
  EmailSendToName,
  getEnumStringOptions,
  Grid,
  IUserReportModel,
  OptionItem,
  ReportDistributionFormatName,
  ReportStatusName,
  Row,
  Select,
  Show,
  Spinner,
  Text,
  useApiAdminUsers,
  validateEmail,
} from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

/**
 * Provides component to enable the user to send their report to subscribers.
 * @returns Component.
 */
export const ReportSendForm: React.FC = () => {
  const { values, isSubmitting, setFieldValue } = useReportEditContext();
  const [{ sendReportInstance }] = useReportInstances();
  const [{ userInfo }] = useApp();
  const { findUsers } = useApiAdminUsers();

  const [to, setTo] = React.useState('');
  const [email, setEmail] = React.useState('');

  const instance = values.instances.length ? values.instances[0] : undefined;
  const isAdmin = userInfo?.roles.includes(Claim.administrator);
  const formatOptions = getEnumStringOptions(ReportDistributionFormatName);
  const sendToOptions = getEnumStringOptions(EmailSendToName, { splitOnCapital: false });

  const handleSend = React.useCallback(
    async (id: number, to: string) => {
      try {
        await sendReportInstance(id, to);
        toast.success('Report has been submitted.');
      } catch {}
    },
    [sendReportInstance],
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
              sendTo: EmailSendToName.To,
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
    <styled.ReportSendForm className="report-send report-edit-section">
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
        <Show visible={instance?.status === ReportStatusName.Submitted}>
          <Col>
            <Spinner />
          </Col>
        </Show>
      </Row>
      <Col className="subscribers">
        <Grid
          items={values.subscribers.filter((s) => s.isSubscribed)}
          renderHeader={() => {
            return [
              <div key="">Username</div>,
              <div key="">Last Name</div>,
              <div key="">First Name</div>,
              <div key="">Email</div>,
              { label: <div key="">Message</div>, size: 'fit-content(35ch)' },
              { label: <div key="">Send as</div>, size: 'fit-content(35ch)' },
              { label: <div key=""></div>, size: '30px' },
            ];
          }}
          renderRow={(row: IUserReportModel, rowIndex) => {
            return [
              <div key="">{row.username}</div>,
              <div key="">{row.lastName}</div>,
              <div key="">{row.firstName}</div>,
              <div key="">{row.preferredEmail.length ? row.preferredEmail : row.email}</div>,
              <div key="">
                <Select
                  name={`subscribers.${rowIndex}.format`}
                  options={formatOptions}
                  isClearable={false}
                  value={formatOptions.find((o) => o.value === row.format) ?? ''}
                  onChange={(newValue) => {
                    const option = newValue as OptionItem;
                    if (option) {
                      setFieldValue(
                        'subscribers',
                        values.subscribers.map((s) =>
                          s.userId === row.userId ? { ...row, format: option.value } : s,
                        ),
                      );
                    }
                  }}
                />
              </div>,
              <div key="">
                <Select
                  name={`subscribers.${rowIndex}.sendTo`}
                  options={sendToOptions}
                  isClearable={false}
                  value={sendToOptions.find((o) => o.value === row.sendTo) ?? ''}
                  onChange={(newValue) => {
                    const option = newValue as OptionItem;
                    if (option) {
                      setFieldValue(
                        'subscribers',
                        values.subscribers.map((s) =>
                          s.userId === row.userId ? { ...row, sendTo: option.value } : s,
                        ),
                      );
                    }
                  }}
                />
              </div>,
              <div key="">
                <Action
                  icon={<FaTrash />}
                  onClick={() =>
                    setFieldValue(
                      'subscribers',
                      values.subscribers.filter((s) => s.userId !== row.userId),
                    )
                  }
                />
              </div>,
            ];
          }}
        />
      </Col>
      <Show visible={isAdmin}>
        <Row gap="1rem">
          <Col flex="1">
            <Section label="Add subscribers to report" open showOpen={false}>
              <Text
                name="email"
                label="Search by email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                width="300px"
              >
                <Button disabled={!validateEmail(email)} onClick={() => addSubscriber(email)}>
                  Add subscriber
                </Button>
              </Text>
            </Section>
          </Col>
          <Col flex="1">
            <Section label="Test" open showOpen={false}>
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
                    >
                      <Button
                        disabled={isSubmitting || !to.length || !validateEmail(to)}
                        onClick={() => !!instance?.id && handleSend(instance.id, to)}
                      >
                        Send email
                        <FaTelegramPlane />
                      </Button>
                    </Text>
                  </Row>
                </Col>
              </Row>
            </Section>
          </Col>
        </Row>
      </Show>
    </styled.ReportSendForm>
  );
};
