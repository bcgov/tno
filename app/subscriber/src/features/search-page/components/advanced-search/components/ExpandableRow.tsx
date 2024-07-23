import React from 'react';
import { FaWater } from 'react-icons/fa6';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';
import { Tooltip } from 'react-tooltip';
import { Col, Row } from 'tno-core';

export interface IExpandableRowProps {
  /** icon to be displayed on the row */
  icon: React.ReactNode;
  /** title of the row */
  title: string;
  /** children to be displayed when the row is expanded */
  children: React.ReactNode;
  /** show icon to indicate values are set inside collapsible row */
  hasValues?: boolean;
}

/** contains the logic and skeleton for an expandable row in the advanced search section. helps to eliminate redundant code. */
export const ExpandableRow: React.FC<IExpandableRowProps> = ({
  children,
  icon,
  title,
  hasValues = false,
}) => {
  const [expanded, setExpanded] = React.useState(false);
  const [uniqueId] = React.useState(new Date().getTime().toString(36));
  return (
    <>
      <Row className="option-row" onClick={() => setExpanded(!expanded)}>
        <Tooltip
          style={{ zIndex: '999' }}
          variant="info"
          id={`btn-tip-${uniqueId}`}
          place="top"
          float
        />
        {icon}
        {title}
        <Col className="action-icons">
          {hasValues ? (
            <FaWater
              className="active-filter-icon"
              data-tooltip-id={`btn-tip-${uniqueId}`}
              data-tooltip-content="Expand to see current filter settings"
            />
          ) : (
            <></>
          )}
          {!expanded ? (
            <IoIosArrowDroprightCircle
              onClick={() => setExpanded(true)}
              className="drop-icon"
              data-tooltip-id={`btn-tip-${uniqueId}`}
              data-tooltip-content="Expand"
            />
          ) : (
            <IoIosArrowDropdownCircle
              onClick={() => setExpanded(false)}
              className="drop-icon"
              data-tooltip-id={`btn-tip-${uniqueId}`}
              data-tooltip-content="Collapse"
            />
          )}
        </Col>
      </Row>
      {expanded && <div className="option-children">{children}</div>}
    </>
  );
};
