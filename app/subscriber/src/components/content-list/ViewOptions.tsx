import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaGear } from 'react-icons/fa6';
import { useUsers } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { Checkbox, Col, ISubscriberUserModel, Radio, Row } from 'tno-core';

import { ContentListContext } from './ContentListContext';
import { IGroupByState, IToggleStates } from './interfaces';
import * as styled from './styled';

export const ViewOptions: React.FC = () => {
  const { viewOptions, setGroupBy, setViewOptions, groupBy } = React.useContext(ContentListContext);
  const [{ userInfo }, store] = useAppStore();
  const [loadShowOptions, setLoadShowOptions] = React.useState(true);
  const api = useUsers();

  /** Save the user's preferences for view/grouping options under preferences */
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const savePreferences = async (viewOptions: IToggleStates, groupBy: IGroupByState) => {
    setLoadShowOptions(false);
    if (userInfo) {
      try {
        const user = {
          ...userInfo,
          preferences: { ...userInfo.preferences, viewOptions, groupBy },
        } as ISubscriberUserModel;
        await api.updateUser(user, userInfo.id);
        store.storeUserInfo({ ...userInfo, preferences: user.preferences });
      } catch (error) {
        console.debug('Is this the optimistic concurrency error?', error);
      }
    }
  };

  /** Set the current state to match the saved preferences if they exist */
  React.useEffect(() => {
    if (userInfo?.preferences?.viewOptions && loadShowOptions) {
      setViewOptions(userInfo.preferences.viewOptions);
    }
    if (userInfo?.preferences?.groupBy && loadShowOptions) {
      setGroupBy(userInfo.preferences.groupBy);
    }
    // only want to run when we have the user's info to init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo]);

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
              const opt = { ...viewOptions, teaser: e.target.checked };
              savePreferences(opt, groupBy);
              setViewOptions(opt);
            }}
          />
          <Checkbox
            label="Sentiment"
            name="sentiment"
            className="checkbox"
            checked={viewOptions.sentiment}
            onChange={(e) => {
              const opt = { ...viewOptions, sentiment: e.target.checked };
              savePreferences(opt, groupBy);
              setViewOptions(opt);
            }}
          />
          <Checkbox
            label="Page numbers"
            name="section"
            className="checkbox"
            checked={viewOptions.section}
            onChange={(e) => {
              const opt = { ...viewOptions, section: e.target.checked };
              savePreferences(opt, groupBy);
              setViewOptions(opt);
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
              onChange={(e) => {
                const grp = e.target.checked ? 'source' : 'time';
                savePreferences(viewOptions, grp);
                setGroupBy(grp);
              }}
            />
            <label htmlFor="source">Media Source</label>
          </Row>
          <Row>
            <Radio
              name="groupBy"
              className="radio"
              id="time"
              checked={groupBy === 'time'}
              onChange={(e) => {
                const grp = e.target.checked ? 'time' : 'source';
                savePreferences(viewOptions, grp);
                setGroupBy(grp);
              }}
            />
            <label htmlFor="time">Time</label>
          </Row>
        </Col>
      </TooltipMenu>
    </styled.ViewOptions>
  );
};
