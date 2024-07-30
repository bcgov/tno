import React from 'react';
import { useSystemMessages } from 'store/hooks';
import { ISystemMessageModel, Show } from 'tno-core';

export const InfoText: React.FC = () => {
  const [, { findSystemMessages }] = useSystemMessages();
  const [systemMessage, setSystemMessage] = React.useState<ISystemMessageModel>();

  React.useEffect(() => {
    findSystemMessages().then((data) => {
      const message = data.find((m) => m.isEnabled);
      if (!!message) setSystemMessage(message);
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
      <Show visible={systemMessage?.isEnabled && !!systemMessage?.message}>
        <div className="system-message">
          <h2>{systemMessage?.name}</h2>
          <div dangerouslySetInnerHTML={{ __html: systemMessage?.message ?? '' }}></div>
        </div>
      </Show>
    </div>
  );
};
