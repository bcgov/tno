import { Action } from 'components/action';
import React from 'react';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';

import * as styled from './styled';

export interface ISectionProps {
  icon?: React.ReactNode;
  label?: React.ReactNode;
  actions?: React.ReactNode;
  open?: boolean;
  showOpen?: boolean;
  children?: React.ReactNode;
  onChange?: (open: boolean) => void;
}

export const Section: React.FC<ISectionProps> = ({
  icon,
  label,
  actions,
  open: initOpen = false,
  showOpen = true,
  children,
  onChange = () => {},
  ...rest
}) => {
  const [open, setOpen] = React.useState(initOpen);

  const handleChange = React.useCallback(
    (open: boolean) => {
      setOpen(open);
      onChange(open);
    },
    [onChange],
  );

  return (
    <styled.Section open={open} {...rest}>
      <div className="section-header">
        {icon && <div className="section-icon">{icon}</div>}
        <div className="section-label">
          <span
            className={`${showOpen ? 'is-clickable' : ''}`}
            onClick={() => showOpen && handleChange(!open)}
          >
            {label}
          </span>
        </div>
        {actions && <div className="section-actions">{actions}</div>}
        {showOpen && (
          <div className="section-open">
            {open ? (
              <Action icon={<FaMinus />} onClick={() => handleChange(false)} />
            ) : (
              <Action icon={<FaAngleDown />} onClick={() => handleChange(true)} />
            )}
          </div>
        )}
      </div>
      {open && <div className="section-body">{children}</div>}
    </styled.Section>
  );
};
