import { defaultSystemMessage } from 'features/admin/system-message/constants';
import parse from 'html-react-parser';
import React from 'react';
import { useAnonSystemMessages } from 'store/hooks';
import { ISystemMessageModel, Show } from 'tno-core';

export const InfoText: React.FC = () => {
  const [, api] = useAnonSystemMessages();
  const [systemMessage, setSystemMessage] =
    React.useState<ISystemMessageModel>(defaultSystemMessage);

  React.useEffect(() => {
    api.findSystemMessage().then((data) => {
      if (!!data) setSystemMessage(data);
    });
    // only want to run on mount
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="info">
      <p>
        Media Monitoring Insights is a service that allows people to see all of BC’s news at a
        glance. Some of its key features include:
      </p>
      <ul>
        <li>Aggregation of all newspapers, radio shows, and online articles.</li>
        <li>BC’s top stories as they break.</li>
        <li>Articles related to major stories.</li>
      </ul>
      <Show visible={systemMessage.isEnabled}>
        <p className="system-message">
          {systemMessage.message ? parse(systemMessage.message) : ''}
        </p>
      </Show>
    </div>
  );
};
