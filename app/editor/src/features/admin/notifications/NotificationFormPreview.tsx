import { AxiosError } from 'axios';
import { useFormikContext } from 'formik';
import React from 'react';
import { toast } from 'react-toastify';
import { useNotifications } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  INotificationModel,
  INotificationResultModel,
  Row,
  Text,
} from 'tno-core';

/**
 * The page used to view and edit reports.
 * @returns Component.
 */
export const NotificationFormPreview: React.FC = () => {
  const { values } = useFormikContext<INotificationModel>();
  const [, { previewNotification, sendNotification }] = useNotifications();

  const [preview, setPreview] = React.useState<INotificationResultModel>();
  const [sendTo, setSendTo] = React.useState({ email: '', contentId: '' });

  const handleSend = async (values: INotificationModel, contentId: number | string, to: string) => {
    try {
      await sendNotification(values, to, !!contentId ? +contentId : undefined);
      toast.success('Notification has been successfully requested');
    } catch {}
  };

  const handlePreviewReport = React.useCallback(
    async (model: INotificationModel, contentId: number) => {
      try {
        const response = await previewNotification(
          {
            ...model,
            instances: [],
            subscribers: [],
          },
          contentId,
        );
        setPreview(response);
      } catch (ex) {
        const error = ex as AxiosError;
        const response = error.response;
        const data = response?.data as any;
        setPreview({
          subject: data.error,
          body: `${data.details}<div>${data.stackTrace}</div>`,
          data: {},
        });
      }
    },
    [previewNotification],
  );

  return (
    <>
      <h2>{values.name}</h2>
      <Row>
        <Col flex="1">
          <p>
            Before saving the notification, generate a preview to ensure it is working and returning
            the correct content.
          </p>
          <p>You can test this notification and send it to the following email address.</p>
        </Col>
        {values.id && (
          <Col>
            <Col>
              <Text
                name="contentId"
                label="Content ID"
                width="15ch"
                type="number"
                value={sendTo.contentId}
                onChange={(e) => {
                  setSendTo({ ...sendTo, contentId: e.target.value });
                }}
              >
                <Button
                  variant={ButtonVariant.success}
                  disabled={!sendTo.contentId}
                  onClick={() => handlePreviewReport(values, +sendTo.contentId)}
                >
                  Generate Preview
                </Button>
              </Text>
            </Col>
            <Col>
              <Text
                name="to"
                label="Send Test Email To"
                value={sendTo.email}
                onChange={(e) => setSendTo({ ...sendTo, email: e.target.value })}
              >
                <Button
                  variant={ButtonVariant.secondary}
                  disabled={!sendTo.email}
                  onClick={async () => await handleSend(values, sendTo.contentId, sendTo.email)}
                >
                  Send
                </Button>
              </Text>
            </Col>
          </Col>
        )}
      </Row>
      <Col className="preview-notification">
        <div
          className="preview-subject"
          dangerouslySetInnerHTML={{ __html: preview?.subject ?? '' }}
        ></div>
        <div
          className="preview-body"
          dangerouslySetInnerHTML={{ __html: preview?.body ?? '' }}
        ></div>
      </Col>
    </>
  );
};
