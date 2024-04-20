import { Action } from 'components/action';
import React from 'react';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';

import * as styled from './styled';

export interface ISectionProps {
  icon?: React.ReactNode;
  label?: React.ReactNode;
  actions?: React.ReactNode | ((props: ISectionProps) => React.ReactNode);
  open?: boolean;
  showOpen?: boolean;
  className?: string;
  children?: React.ReactNode | ((props: ISectionProps) => React.ReactNode);
  onChange?: (open: boolean) => void;
}

export const Section: React.FC<ISectionProps> = ({
  icon,
  label,
  actions,
  open: initOpen = false,
  showOpen = true,
  className = '',
  children,
  onChange = () => {},
  ...rest
}) => {
  const [open, setOpen] = React.useState(initOpen);

  React.useEffect(() => {
    setOpen(initOpen);
  }, [initOpen]);

  const handleChange = React.useCallback(
    (open: boolean) => {
      setOpen(open);
      onChange(open);
    },
    [onChange],
  );

  return (
    <styled.Section open={open} showOpen={showOpen} className={`section ${className}`} {...rest}>
      <div className="section-header">
        {open && <div className="section-active">&nbsp;</div>}
        {icon && <div className="section-icon">{icon}</div>}
        <div className="section-label" onClick={() => showOpen && handleChange(!open)}>
          {label}
        </div>
        {actions && (
          <div className="section-actions">
            {typeof actions === 'function'
              ? (actions as (props: ISectionProps) => React.ReactNode)({ open, showOpen, ...rest })
              : actions}
          </div>
        )}
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
      {open && (
        <div className="section-body">
          {typeof children === 'function'
            ? (children as (props: ISectionProps) => React.ReactNode)({
                icon,
                label,
                actions,
                open,
                showOpen,
                className,
                children,
                onChange,
                ...rest,
              })
            : children}
        </div>
      )}
    </styled.Section>
  );
};
