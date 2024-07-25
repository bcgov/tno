import { Action } from 'components/action';
import { PageSection } from 'components/section';
import { MyColleaguesLanding } from 'features/my-colleagues';
import React from 'react';
import { FaCog } from 'react-icons/fa';
import { Col, Row, Show } from 'tno-core';

import { SettingsSessionEnum } from './constants/SettingsSessionEnum';
import { MyMinisterSettings } from './MyMinisterSettings';
import * as styled from './styled';

export const SettingsLanding: React.FC<{}> = () => {
  const [session, setSession] = React.useState('');

  const show = (session: string) => {
    setSession(session);
  };

  return (
    <styled.SettingsLanding split={session !== ''}>
      <Col className="left-side">
        <PageSection header="Settings" includeHeaderIcon className="menuPanel">
          <div className="link-box-container">
            <div className="link-box" onClick={() => show(SettingsSessionEnum.MyMinister)}>
              <h2>
                <FaCog className="list-icon" />
                <span>My Minister</span>
              </h2>
              <div className="description">
                Choose the Minister(s) you like to follow. Stories about your chosen Minister(s)
                will then be avaliable from "My Minister" on the sidebar menu.
              </div>
            </div>
            <div className="link-box" onClick={() => show(SettingsSessionEnum.MyColleagues)}>
              <h2>
                <FaCog className="list-icon" />
                <span>My Colleagues</span>
              </h2>
              <div className="description">
                Add MMI users that you would like to be able to quickly share stories with. From any
                of the story listings, or from the story itself, you can then choose "share" and
                find your colleagues in this list.
              </div>
            </div>
          </div>
        </PageSection>
      </Col>
      <Show visible={true}>
        <Col className="right-side">
          <Show visible={session === SettingsSessionEnum.MyMinister}>
            <PageSection
              header={
                <Row className="header-row">
                  <div className="title">My Minister</div>
                  <Action
                    variant="close"
                    className="close-button"
                    title="Revert"
                    onClick={() => setSession('')}
                  />
                </Row>
              }
              includeHeaderIcon
            >
              <MyMinisterSettings />
            </PageSection>
          </Show>
          <Show visible={session === SettingsSessionEnum.MyColleagues}>
            <PageSection
              header={
                <Row className="header-row">
                  <div className="title">My Colleagues</div>
                  <Action
                    variant="close"
                    className="close-button"
                    title="Revert"
                    onClick={() => setSession('')}
                  />
                </Row>
              }
              includeHeaderIcon
            >
              <MyColleaguesLanding inFrame={true} />
            </PageSection>
          </Show>
        </Col>
      </Show>
    </styled.SettingsLanding>
  );
};
