import React from 'react';
import { FaBell } from 'react-icons/fa6';
import { useSystemMessages } from 'store/hooks';
import { Col, ISystemMessageModel, Show } from 'tno-core';

import * as styled from './styled';

export const SystemMessage: React.FC = () => {
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
    <styled.SystemMessage>
      <Show visible={!!systemMessage && systemMessage.isEnabled}>
        <Col className="system-message-box">
          <div className="system-message-containing-box">
            <b className="alert-title">System Alerts &amp; Info</b>
            <FaBell className="bell-icon" />
            <div
              dangerouslySetInnerHTML={{
                __html: systemMessage?.message ?? '',
              }}
            ></div>
          </div>
        </Col>
      </Show>
    </styled.SystemMessage>
  );
};
