import 'prismjs/themes/prism.css';
import 'prismjs/components/prism-csharp';
import 'prismjs/components/prism-cshtml';
import 'prismjs/components/prism-json';

import { FormikForm } from 'components/formik';
import React from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp } from 'store/hooks';
import { useNotifications } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  IconButton,
  INotificationModel,
  Modal,
  Row,
  Show,
  Tab,
  Tabs,
  useModal,
} from 'tno-core';

import { defaultNotification, defaultNotificationTemplate } from './constants';
import { NotificationFilterForm } from './NotificationFilterForm';
import { NotificationFormDetails } from './NotificationFormDetails';
import { NotificationFormPreview } from './NotificationFormPreview';
import { NotificationFormTemplate } from './NotificationFormTemplate';
import { NotificationFormTestFilter } from './NotificationFormTestFilter';
import { NotificationSubscribersForm } from './NotificationSubscribersForm';
import * as styled from './styled';

/**
 * The page used to view and edit Notifications.
 * @returns Component.
 */
const NotificationForm: React.FC = () => {
  const navigate = useNavigate();
  const [{ userInfo }] = useApp();
  const { id } = useParams();
  const [, { getNotification, addNotification, updateNotification, deleteNotification }] =
    useNotifications();
  const { state } = useLocation();
  const { toggle, isShowing } = useModal();

  const [Notification, setNotification] = React.useState<INotificationModel>(
    (state as any)?.Notification ?? { ...defaultNotification, ownerId: userInfo?.id },
  );
  const [active, setActive] = React.useState('Notification');

  const NotificationId = Number(id);

  React.useEffect(() => {
    if (!!NotificationId && Notification?.id !== NotificationId) {
      setNotification({ ...defaultNotification, id: NotificationId }); // Do this to stop double fetch.
      getNotification(NotificationId).then((data) => {
        setNotification(data);
      });
    }
  }, [getNotification, Notification?.id, NotificationId]);

  const handleSubmit = React.useCallback(
    async (values: INotificationModel) => {
      try {
        const originalId = values.id;
        const notification: INotificationModel = values.templateId
          ? values
          : {
              ...values,
              template: {
                ...(values.template ?? defaultNotificationTemplate),
                name: `${values.name}-${Date.now().toString()}`,
              },
            };
        const result = !Notification.id
          ? await addNotification({
              ...notification,
              ownerId: notification.ownerId ?? userInfo?.id,
            })
          : await updateNotification(notification);
        setNotification(result);
        toast.success(`${result.name} has successfully been saved.`);
        if (!originalId) navigate(`/admin/Notifications/${result.id}`);
      } catch {}
    },
    [Notification.id, addNotification, navigate, updateNotification, userInfo?.id],
  );

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
                  label="Template"
                  onClick={() => {
                    setActive('template');
                  }}
                  active={active === 'template'}
                />
                <Tab
                  label="Filter"
                  onClick={() => {
                    setActive('filter');
                  }}
                  active={active === 'filter'}
                />
                <Tab
                  label="Test Filter"
                  onClick={() => {
                    setActive('test');
                  }}
                  active={active === 'test'}
                />
                <Tab
                  label="Preview"
                  onClick={() => {
                    setActive('preview');
                  }}
                  active={active === 'preview'}
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
                <NotificationFormDetails />
              </Show>
              <Show visible={active === 'template'}>
                <NotificationFormTemplate />
              </Show>
              <Show visible={active === 'filter'}>
                <NotificationFilterForm />
              </Show>
              <Show visible={active === 'test'}>
                <NotificationFormTestFilter />
              </Show>
              <Show visible={active === 'preview'}>
                <NotificationFormPreview />
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
                    await deleteNotification(Notification);
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
