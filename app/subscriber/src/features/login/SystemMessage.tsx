import parse from 'html-react-parser';
import React from 'react';
import { useSystemMessages } from 'store/hooks';
import { Col, ISystemMessageModel, Show } from 'tno-core';

import * as styled from './styled';

export const SystemMessage: React.FC = () => {
  const [, api] = useSystemMessages();
  const [systemMessage, setSystemMessage] = React.useState<ISystemMessageModel>();

  React.useEffect(() => {
    api.findSystemMessage().then((data) => {
      if (!!data) setSystemMessage(data);
    });
    // only want to run on mount
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <styled.SystemMessage>
      <Show visible={!!systemMessage?.message && systemMessage.isEnabled}>
        <Col className="system-message-box">
          <div className="system-message-containing-box">
            <b className="alert-title">System Alerts &amp; Info</b>
            <p>{parse(systemMessage?.message ?? '')}</p>
          </div>
        </Col>
      </Show>
    </styled.SystemMessage>
  );
};
