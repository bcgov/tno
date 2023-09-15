import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { FormikForm } from 'components/formik';
import { noop } from 'lodash';
import moment from 'moment';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useNotifications } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getEnumStringOptions,
  IconButton,
  INotificationModel,
  Modal,
  NotificationTypeName,
  ResendOptionName,
  Row,
  Show,
  Tab,
  Tabs,
  Text,
  useModal,
} from 'tno-core';

import { defaultNotification } from './constants';
import { NotificationFilterForm } from './NotificationFilterForm';
import { NotificationSubscribersForm } from './NotificationSubscribersForm';
import { NotificationTemplateForm } from './NotificationTemplateForm';
import * as styled from './styled';

/**
 * The page used to view and edit Notifications.
 * @returns Component.
 */
const NotificationForm: React.FC = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, api] = useNotifications();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();

  const [Notification, setNotification] = React.useState<INotificationModel>(
    (state as any)?.Notification ?? { ...defaultNotification, ownerId: userInfo?.id ?? 0 },
  );
  const [sendTo, setSendTo] = React.useState({ email: '', contentId: '' });
  const [active, setActive] = React.useState('Notification');

  const NotificationId = Number(id);
  const NotificationTypeOptions = getEnumStringOptions(NotificationTypeName);
  const resendOptions = getEnumStringOptions(ResendOptionName);

  React.useEffect(() => {
    if (!!NotificationId && Notification?.id !== NotificationId) {
      setNotification({ ...defaultNotification, id: NotificationId }); // Do this to stop double fetch.
      api.getNotification(NotificationId).then((data) => {
        setNotification(data);
      });
    }
  }, [api, Notification?.id, NotificationId]);

  const handleSubmit = async (values: INotificationModel) => {
    try {
      const originalId = values.id;
      const result = !Notification.id
        ? await api.addNotification(values)
        : await api.updateNotification(values);
      setNotification(result);
      toast.success(`${result.name} has successfully been saved.`);
      if (!originalId) navigate(`/admin/Notifications/${result.id}`);
    } catch {}
  };

  const handleSend = async (values: INotificationModel, contentId: number | string, to: string) => {
    try {
      if (!!contentId) {
        await api.sendNotification(values, +contentId, to);
      }
      toast.success('Notification has been successfully requested');
    } catch {}
  };

  return (
    <styled.NotificationForm>
      <IconButton
        iconType="back"
        label="Back to Notifications"
        className="back-button"
        onClick={() => navigate('/admin/Notifications')}
      />
      <FormikForm
        initialValues={Notification}
        onSubmit={(values, { setSubmitting }) => {
          handleSubmit(values);
          setSubmitting(false);
        }}
      >
        {({ isSubmitting, values, setFieldValue }) => (
          <Tabs
            tabs={
              <>
                <Tab
                  label="Notification"
                  onClick={() => {
                    setActive('Notification');
                  }}
                  active={active === 'Notification'}
                />
                <Tab
                  label="Filter"
                  onClick={() => {
                    setActive('filter');
                  }}
                  active={active === 'filter'}
                />
                <Tab
                  label="Template"
                  onClick={() => {
                    setActive('template');
                  }}
                  active={active === 'template'}
                />
                <Tab
                  label="Subscribers"
                  onClick={() => {
                    setActive('subscribers');
                  }}
                  active={active === 'subscribers'}
                />
              </>
            }
          >
            <div className="form-container">
              <Show visible={active === 'Notification'}>
                <Col className="form-inputs">
                  <FormikText name="name" label="Name" />
                  <FormikTextArea name="description" label="Description" />
                  <Row>
                    <FormikSelect
                      name="notificationType"
                      label="Notification Type"
                      options={NotificationTypeOptions}
                      required
                      isClearable={false}
                    />
                    <FormikSelect
                      name="resend"
                      label="Resend Option"
                      options={resendOptions}
                      required
                      isClearable={false}
                    />
                    <FormikText
                      width={FieldSize.Tiny}
                      name="sortOrder"
                      label="Sort Order"
                      type="number"
                      className="sort-order"
                    />
                  </Row>
                  <Row>
                    <Col flex="1">
                      <Row gap="1rem">
                        <Col>
                          <FormikCheckbox
                            label="Require Content to be Alerted"
                            name="requireAlert"
                          />
                          <FormikCheckbox label="Is Enabled" name="isEnabled" />
                          <FormikCheckbox label="Is Public" name="isPublic" />
                        </Col>
                      </Row>
                      <Show visible={!!values.id}>
                        <Row>
                          <FormikText
                            width={FieldSize.Small}
                            disabled
                            name="updatedBy"
                            label="Updated By"
                          />
                          <FormikDatePicker
                            selectedDate={
                              !!values.updatedOn ? moment(values.updatedOn).toString() : undefined
                            }
                            onChange={noop}
                            name="updatedOn"
                            label="Updated On"
                            disabled
                            width={FieldSize.Small}
                          />
                        </Row>
                        <Row>
                          <FormikText
                            width={FieldSize.Small}
                            disabled
                            name="createdBy"
                            label="Created By"
                          />
                          <FormikDatePicker
                            selectedDate={
                              !!values.createdOn ? moment(values.createdOn).toString() : undefined
                            }
                            onChange={noop}
                            name="createdOn"
                            label="Created On"
                            disabled
                            width={FieldSize.Small}
                          />
                        </Row>
                      </Show>
                    </Col>
                    {values.id && (
                      <Col flex="1">
                        <h2>Test Notification</h2>
                        <p>
                          You can test this Notification and send it to the following email address.
                        </p>
                        <Row>
                          <Col>
                            <Text
                              name="contentId"
                              label="Content ID"
                              width="10ch"
                              type="number"
                              value={sendTo.contentId}
                              onChange={(e) => {
                                setSendTo({ ...sendTo, contentId: e.target.value });
                              }}
                            ></Text>
                          </Col>
                          <Col>
                            <Text
                              name="to"
                              label="Email To"
                              value={sendTo.email}
                              onChange={(e) => setSendTo({ ...sendTo, email: e.target.value })}
                            >
                              <Button
                                variant={ButtonVariant.secondary}
                                disabled={!sendTo}
                                onClick={async () =>
                                  await handleSend(values, sendTo.contentId, sendTo.email)
                                }
                              >
                                Send
                              </Button>
                            </Text>
                          </Col>
                        </Row>
                      </Col>
                    )}
                  </Row>
                </Col>
              </Show>
              <Show visible={active === 'filter'}>
                <NotificationFilterForm />
              </Show>
              <Show visible={active === 'template'}>
                <NotificationTemplateForm />
              </Show>
              <Show visible={active === 'subscribers'}>
                <NotificationSubscribersForm />
              </Show>
              <Row justifyContent="center" className="form-inputs">
                <Button type="submit" disabled={isSubmitting}>
                  Save
                </Button>
                <Show visible={!!values.id}>
                  <Button onClick={toggle} variant={ButtonVariant.danger} disabled={isSubmitting}>
                    Delete
                  </Button>
                </Show>
              </Row>
              <Modal
                headerText="Confirm Removal"
                body="Are you sure you wish to remove this notification?"
                isShowing={isShowing}
                hide={toggle}
                type="delete"
                confirmText="Yes, Remove It"
                onConfirm={async () => {
                  try {
                    await api.deleteNotification(Notification);
                    toast.success(`${Notification.name} has successfully been deleted.`);
                    navigate('/admin/notifications');
                  } finally {
                    toggle();
                  }
                }}
              />
            </div>
          </Tabs>
        )}
      </FormikForm>
    </styled.NotificationForm>
  );
};

export default NotificationForm;
