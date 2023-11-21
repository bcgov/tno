import React from 'react';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';
import { Row } from 'tno-core';

export interface IExpandableRowProps {
  /** icon to be displayed on the row */
  icon: React.ReactNode;
  /** title of the row */
  title: string;
  /** children to be displayed when the row is expanded */
  children: React.ReactNode;
}

/** contains the logic and skeleton for an expandable row in the advanced search section. helps to eliminate redundant code. */
export const ExpandableRow: React.FC<IExpandableRowProps> = ({ children, icon, title }) => {
  const [expanded, setExpanded] = React.useState(false);
  return (
    <>
      <Row className="option-row" onClick={() => setExpanded(!expanded)}>
        {icon}
        {title}
        {!expanded ? (
          <IoIosArrowDroprightCircle onClick={() => setExpanded(true)} className="drop-icon" />
        ) : (
          <IoIosArrowDropdownCircle onClick={() => setExpanded(false)} className="drop-icon" />
        )}
      </Row>
      {expanded && <div className="option-children">{children}</div>}
    </>
  );
};
