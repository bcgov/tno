import { PageSection } from 'components/section';
import React from 'react';
import { FaCog } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { Col, Show } from 'tno-core';

import { MyMinisterSettings } from './MyMinisterSettings';
import * as styled from './styled';

export const SettingsLanding: React.FC<{}> = () => {
  const { action } = useParams();

  return (
    <styled.SettingsLanding split={true}>
      <Col className="left-side">
        <PageSection header="Settings" includeHeaderIcon>
          <p>
            <FaCog /> My Minister
          </p>
          <span>
            Choose the Minister(s) you like to follow. Stories about your chosen Minister(s) will
            then be avaliable from "My Minister" on the sidebar menu.
          </span>
          <p>
            <FaCog /> My Colleagues
          </p>
          <span>
            Add MMI users that you would like to be able to quickly share stories with. From any of
            the story listings, or from the story itself, you can then choose "share" and find your
            colleagues in this list.
          </span>
        </PageSection>
      </Col>
      <Show visible={true}>
        <Col className="right-side">
          <PageSection header="My Minister" includeHeaderIcon>
            <MyMinisterSettings />
          </PageSection>
        </Col>
      </Show>
    </styled.SettingsLanding>
  );
};
