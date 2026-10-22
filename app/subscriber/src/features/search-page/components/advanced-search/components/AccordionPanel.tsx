import React from 'react';
import { FaWater } from 'react-icons/fa6';
import { IoIosArrowDropdownCircle, IoIosArrowDroprightCircle } from 'react-icons/io';

import * as styled from './styled';

export interface IAccordionPanelProps {
  /** Identifies the panel; used for its element ids. */
  id: string;
  /** Icon shown before the title. */
  icon: React.ReactNode;
  title: string;
  /** Whether the panel's body is shown. */
  open: boolean;
  /** Show an icon to indicate values are set inside the panel. */
  hasValues?: boolean;
  /** The header was clicked. */
  onToggle: () => void;
  children: React.ReactNode;
}

/** One panel of an accordion: a header that opens and closes its body. */
export const AccordionPanel: React.FC<IAccordionPanelProps> = ({
  id,
  icon,
  title,
  open,
  hasValues = false,
  onToggle,
  children,
}) => {
  return (
    <styled.AccordionPanel className={`accordion-panel${open ? ' open' : ''}`}>
      <button
        type="button"
        id={`${id}-header`}
        className="accordion-header"
        aria-expanded={open}
        aria-controls={`${id}-body`}
        onClick={onToggle}
      >
        {icon}
        <span className="accordion-title">{title}</span>
        {hasValues && <FaWater className="active-filter-icon" title="This panel has values set" />}
        {open ? (
          <IoIosArrowDropdownCircle className="drop-icon" />
        ) : (
          <IoIosArrowDroprightCircle className="drop-icon" />
        )}
      </button>
      {open && (
        <div
          id={`${id}-body`}
          className="accordion-body"
          role="region"
          aria-labelledby={`${id}-header`}
        >
          {children}
        </div>
      )}
    </styled.AccordionPanel>
  );
};
