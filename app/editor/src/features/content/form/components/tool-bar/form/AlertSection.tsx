import { ContentActions } from 'features/content/form';
import { IContentForm } from 'features/content/form/interfaces';
import React from 'react';
import { FaPaperPlane } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useApiHub, useContent, useLookup } from 'store/hooks';
import {
  Col,
  IContentActionMessageModel,
  INotificationInstanceModel,
  MessageTargetKey,
  NotificationStatusName,
  Row,
  Settings,
  ToolBarSection,
} from 'tno-core';

export interface IAlertSectionProps {
  /** Form values. */
  values: IContentForm;
}

export const AlertSection = React.forwardRef<HTMLDivElement, IAlertSectionProps>(
  ({ values }, ref) => {
    const [{ actions, settings }] = useLookup();
    const [, { getNotificationsFor }] = useContent();
    const hub = useApiHub();

    const [notifications, setNotifications] = React.useState<INotificationInstanceModel[]>([]);

    const alertId = settings.find((s) => s.name === Settings.DefaultAlert)?.value ?? '0';
    const hasAlert =
      actions.find((a) => a.id === +alertId)?.contentTypes.includes(values.contentType) ?? false;

    const onContentAction = React.useCallback(
      (action: IContentActionMessageModel) => {
        if (action.contentId === values.id) {
          getNotificationsFor(values.id)
            .then((notifications) => {
              setNotifications(notifications);
              toast.info(`Alert has successfully been sent`);
            })
            .catch(() => {});
        }
      },
      [getNotificationsFor, values.id],
    );

    hub.useHubEffect(MessageTargetKey.ContentActionUpdated, onContentAction);

    React.useEffect(() => {
      if (values.id)
        getNotificationsFor(values.id)
          .then((notifications) => setNotifications(notifications))
          .catch(() => {});
    }, [getNotificationsFor, values.id]);

    if (!hasAlert) return null;

    var status = notifications.length ? notifications[0].status : 'not sent';
    if (status === NotificationStatusName.Completed) status = 'Sent';

    return (
      <ToolBarSection
        className="toolbar-alert"
        label="EMAIL ALERT"
        icon={<FaPaperPlane />}
        children={
          <Col gap="0.5rem">
            <ContentActions filter={(a) => a.id === +alertId} ref={ref} />
            <Row className="white-bg" justifyContent="center">
              {status}
            </Row>
          </Col>
        }
      />
    );
  },
);
