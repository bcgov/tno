import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaGear } from 'react-icons/fa6';
import { useUsers } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Checkbox, Col, IUserModel, Radio, Row } from 'tno-core';

import { ContentListContext } from './ContentListContext';
import * as styled from './styled';

export const ViewOptions: React.FC = () => {
  const { viewOptions, setGroupBy, setViewOptions, groupBy } = React.useContext(ContentListContext);
  const [{ userInfo }, store] = useAppStore();
  const api = useUsers();

  /** Save the user's preferences for view/grouping options under preferences */
  const savePreferences = async () => {
    if (userInfo) {
      const user = {
        ...userInfo,
        preferences: { ...userInfo.preferences, viewOptions, groupBy },
        isSystemAccount: false,
        emailVerified: false,
        uniqueLogins: 0,
      } as IUserModel;
      await api.updateUser(user, userInfo.id);
      store.storeUserInfo({ ...userInfo, preferences: user.preferences });
    }
  };

  /** Set the current state to match the saved preferences if they exist */
  React.useEffect(() => {
    if (userInfo?.preferences?.viewOptions) {
      setViewOptions(userInfo.preferences.viewOptions);
    }
    if (userInfo?.preferences?.groupBy) {
      setGroupBy(userInfo.preferences.groupBy);
    }
  }, [userInfo]);

  /** Save the user's preferences when they change */
  React.useEffect(() => {
    savePreferences();
  }, [viewOptions, groupBy]);

  return (
    <styled.ViewOptions className="view-options">
      <FaGear className="gear" data-tooltip-id="view-options" />
      <TooltipMenu clickable openOnClick place="right" id="view-options">
        <div className="show">
          <h3>Show:</h3>
          <Checkbox
            label="Teasers"
            checked={viewOptions.teaser}
            name="teaser"
            className="checkbox"
            onChange={(e) => {
              setViewOptions({ ...viewOptions, teaser: e.target.checked });
            }}
          />
          <Checkbox
            label="Sentiment"
            name="sentiment"
            className="checkbox"
            checked={viewOptions.sentiment}
            onChange={(e) => {
              setViewOptions({ ...viewOptions, sentiment: e.target.checked });
            }}
          />
          <Checkbox
            label="Page numbers"
            name="section"
            className="checkbox"
            checked={viewOptions.section}
            onChange={(e) => {
              setViewOptions({ ...viewOptions, section: e.target.checked });
            }}
          />
        </div>
        <Col className="group-by">
          <h3>Organize By:</h3>
          <Row>
            <Radio
              className="radio"
              name="groupBy"
              id="source"
              checked={groupBy === 'source'}
              onChange={(e) => setGroupBy(e.target.checked ? 'source' : 'time')}
            />
            <label htmlFor="source">Media Source</label>
          </Row>
          <Row>
            <Radio
              name="groupBy"
              className="radio"
              id="time"
              checked={groupBy === 'time'}
              onChange={(e) => setGroupBy(e.target.checked ? 'time' : 'source')}
            />
            <label htmlFor="time">Time</label>
          </Row>
        </Col>
      </TooltipMenu>
    </styled.ViewOptions>
  );
};
