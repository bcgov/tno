import { Action } from 'components/action';
import { Button } from 'components/button';
import React from 'react';
import { FaCopy, FaTrash, FaUserPlus, FaUsers } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import {
  Checkbox,
  Claim,
  Col,
  EmailSendToName,
  getEnumStringOptions,
  Grid,
  IUserReportModel,
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
import { ReportSubscriberExporter } from '../send/ReportSubscriberExporter';
import * as styled from './styled';
export const ReportEditSubscribersForm = () => {
  const { values, setFieldValue } = useReportEditContext();
  const [{ userInfo }] = useApp();
  const { findUsers } = useApiAdminUsers();
  const [emailForAdd, setEmailForAdd] = React.useState('');
  const [emailForRequest, setEmailForRequest] = React.useState('');

  const instance = values.instances.length ? values.instances[0] : undefined;
  const isAdmin = userInfo?.roles.includes(Claim.administrator);
  const formatOptions = getEnumStringOptions(ReportDistributionFormatName);
  const sendToOptions = getEnumStringOptions(EmailSendToName, { splitOnCapital: false });
  const [selectedSubscribers, setSelectedSubscribers] = React.useState<number[]>([]);

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
  const handleSelectSubscriber = (userId: any) => {
    setSelectedSubscribers((prevSelected: any) =>
      prevSelected.includes(userId)
        ? prevSelected.filter((id: any) => id !== userId)
        : [...prevSelected, userId],
    );
  };
  const handleSelectAllSubscribers = () => {
    if (selectedSubscribers.length === values.subscribers.length) {
      setSelectedSubscribers([]);
    } else {
      setSelectedSubscribers(values.subscribers.map((subscriber) => subscriber.userId));
    }
  };

  const handleCopySelected = () => {
    const selectedData = values.subscribers
      .filter((subscriber) => selectedSubscribers.includes(subscriber.userId))
      .map(({ email }) => email)
      .join(',');
    navigator.clipboard.writeText(selectedData);
    toast.success('Selected subscribers copied to clipboard.');
  };

  return (
    <styled.ReportEditSubscribersForm className="report-send report-edit-section">
      <Row>
        <Show visible={instance?.status === ReportStatusName.Submitted}>
          <Col>
            <Spinner />
          </Col>
        </Show>
      </Row>
      <Show visible={isAdmin}>
        <Row gap="1rem">
          <Col flex="1">
            <div className="subscriber-block">
              <div>
                <FaUserPlus size={20} />
              </div>
              <Col>
                <div className="subscriber-title">Add a Subscriber</div>
                <div className="subscriber-describe">
                  Subscribers will receive this report by email each time it is sent out. To be
                  added, a person must have an active MMI account (direct or indirect).
                </div>{' '}
                <Text
                  name="email"
                  label="Add a Subscriber"
                  value={emailForAdd}
                  onChange={(e) => setEmailForAdd(e.target.value)}
                  width="300px"
                >
                  <Button
                    className="request-button"
                    variant="secondary"
                    disabled={!validateEmail(emailForAdd)}
                    onClick={() => addSubscriber(emailForAdd)}
                    style={{ backgroundColor: 'transparent' }}
                  >
                    <FaUserPlus />
                    Add
                  </Button>
                </Text>
              </Col>
            </div>
          </Col>
        </Row>
      </Show>
      <Row gap="1rem">
        <Col flex="1">
          <div className="subscriber-block">
            <div>
              <FaUserPlus size={20} />
            </div>
            <Col>
              <div className="subscriber-title">Request to Add a Subscriber</div>
              <div className="subscriber-describe">
                Subscribers will receive this report by email each time it is sent out. To be added,
                a person must have an active MMI account (direct or indirect).
              </div>{' '}
              <Text
                name="email"
                label="Add a Subscriber"
                value={emailForRequest}
                onChange={(e) => setEmailForRequest(e.target.value)}
                width="300px"
              >
                <Button
                  className="request-button"
                  variant="secondary"
                  disabled={!validateEmail(emailForRequest)}
                  onClick={() => {}}
                  style={{ backgroundColor: 'transparent' }}
                >
                  Send Request
                </Button>
              </Text>
            </Col>
          </div>
        </Col>
      </Row>
      <Row>
        <div className="subscriber-block">
          <FaUsers size={20} />
          <Col className="subscribers">
            <Row justifyContent="space-between">
              <div className="subscriber-title">Current Subscribers</div>
              <div className="report-exporter-container">
                <div
                  className={`subscriber-exporter ${
                    values?.subscribers?.length === 0 ? 'empty' : ''
                  }`}
                >
                  <ReportSubscriberExporter />
                </div>
              </div>
            </Row>

            {selectedSubscribers.length > 0 ? (
              <Button className="selected-all-btn" onClick={handleCopySelected}>
                <FaCopy /> Copy Selected
              </Button>
            ) : (
              <Button className="select-all-btn" disabled={true}>
                <FaCopy />
              </Button>
            )}

            <Grid
              items={values.subscribers.filter((s) => s.isSubscribed)}
              renderHeader={() => {
                return [
                  {
                    name: 'select',
                    label: (
                      <Checkbox
                        checked={
                          selectedSubscribers.length === values.subscribers.length &&
                          values.subscribers.length > 0
                        }
                        onChange={handleSelectAllSubscribers}
                      />
                    ),
                    size: '3rem',
                  },

                  <div key="username">Username</div>,
                  <div key="firstName">First Name</div>,
                  <div key="lastName">Last Name</div>,
                  <div key="email">Email</div>,
                  <div key="format">Format</div>,
                  <div key="sendTo">Send as</div>,
                  <div key="actions">Actions</div>,
                ];
              }}
              renderColumns={(row, rowIndex) => {
                return [
                  <div key="select">
                    <Checkbox
                      checked={selectedSubscribers.includes(row.userId)}
                      onChange={() => handleSelectSubscriber(row.userId)}
                    />
                  </div>,
                  <div key="username">{row.username}</div>,
                  <div key="firstName">{row.firstName}</div>,
                  <div key="lastName">{row.lastName}</div>,
                  <div key="email">
                    {row.preferredEmail.length ? row.preferredEmail : row.email}
                  </div>,
                  <div key="format">
                    <Select
                      name={`subscribers.${rowIndex}.format`}
                      options={formatOptions}
                      isClearable={false}
                      value={formatOptions.find((o) => o.value === row.format) ?? ''}
                      onChange={(newValue: any) => {
                        const option = newValue;
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
                  <div key="sendTo">
                    <Select
                      name={`subscribers.${rowIndex}.sendTo`}
                      options={sendToOptions}
                      isClearable={false}
                      value={sendToOptions.find((o) => o.value === row.sendTo) ?? ''}
                      onChange={(newValue: any) => {
                        const option = newValue;
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
                  <div key="actions">
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
        </div>
      </Row>
    </styled.ReportEditSubscribersForm>
  );
};
