import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { FaGear } from 'react-icons/fa6';
import { Checkbox, Col, Radio, Row } from 'tno-core';

import { ContentListContext } from './ContentListContext';
import * as styled from './styled';

export const ViewOptions: React.FC = () => {
  const { viewOptions, setGroupBy, setViewOptions, groupBy } = React.useContext(ContentListContext);
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
