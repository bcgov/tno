import React from 'react';
import { FaCircleChevronDown, FaCircleChevronUp } from 'react-icons/fa6';
import { INotificationInstanceModel, NotificationStatusName, TextArea } from 'tno-core';

import { getLastSent, getStatus } from './utils';

export interface INotificationCardProps {
  instance: INotificationInstanceModel;
}

export const NotificationCard: React.FC<INotificationCardProps> = ({ instance }) => {
  const [expand, setExpand] = React.useState(false);
  const [expandResponse, setExpandResponse] = React.useState<Record<number, boolean>>({});

  const stringifyResponse = React.useCallback((data: any) => {
    try {
      return data ? JSON.stringify(data, undefined, `\t`) : '';
    } catch {
      return '';
    }
  }, []);

  return (
    <div className={`notification-card`}>
      <div>{instance.notification.name}</div>
      <div>{instance.subject}</div>
      <div>{instance.notification.owner?.email}</div>
      <div className={`${instance?.status === NotificationStatusName.Failed ? 'failed' : ''}`}>
        {getStatus(instance?.status)}
      </div>
      <div>{getLastSent(instance)}</div>
      <div className="buttons">
        <div onClick={() => setExpand((value) => !value)}>
          {expand ? <FaCircleChevronUp /> : <FaCircleChevronDown />}
        </div>
      </div>
      {expand && (
        <div className="full-width">
          <div className="response">
            <div
              className="buttons left"
              onClick={() =>
                setExpandResponse((value) => ({
                  ...value,
                  [instance.id]: !value[instance.id],
                }))
              }
            >
              <h3>CHES Response</h3>
              <div>
                {expandResponse[instance.id] ? <FaCircleChevronUp /> : <FaCircleChevronDown />}
              </div>
            </div>
            {expandResponse[instance.id] && (
              <div>
                <TextArea name="response" value={stringifyResponse(instance?.response)} />
              </div>
            )}
          </div>
          <h3>Subscribers</h3>
          <div className="subscribers">
            {!instance.notification.subscribers.length && (
              <div className="subscriber">
                {!instance.notification.subscribers.length && 'No Subscribers'}
              </div>
            )}
            {instance.notification.subscribers.map((subscriber) => {
              return (
                <div key={subscriber.id} className="subscriber">
                  <div>{subscriber.email}</div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
};
