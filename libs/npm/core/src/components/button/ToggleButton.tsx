import React from 'react';

import * as styled from './styled';

export interface IToggleButtonProps {
  /** Icon to show when 'value=true' */
  on: React.ReactNode;
  /** Label to show when 'value=true' */
  onLabel?: React.ReactNode;
  /** Icon to show when 'value=false' */
  off: React.ReactNode;
  /** Label to show when 'value=false' */
  offLabel?: React.ReactNode;
  /** The current value. */
  value?: boolean;
  /** Whether the button is disabled. */
  disabled?: boolean;
  /** The class name. */
  className?: string;
  /** Width of the icon. */
  width?: string;
  /** Height of the icon. */
  height?: string;
  /** A label for the button. */
  label?: React.ReactNode;
  /** Position of the label. */
  labelPosition?: 'left' | 'right' | 'top' | 'bottom';
  /** Event fires when button clicked. */
  onClick?: React.MouseEventHandler<HTMLDivElement>;
}

/**
 * Provides a button that toggles between two states each time it is clicked.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ToggleButton = ({
  on,
  onLabel,
  off,
  offLabel,
  value,
  disabled,
  className,
  label,
  labelPosition = 'right',
  onClick,
  ...rest
}: IToggleButtonProps) => {
  const [show, setShow] = React.useState(value);

  React.useEffect(() => {
    setShow(value);
  }, [value]);

  return (
    <styled.ToggleButton
      className={`btn-toggle${className ? ` ${className}` : ''}`}
      disabled={disabled}
      labelPosition={labelPosition}
      {...rest}
    >
      <div
        onClick={(e) => {
          setShow(!show);
          onClick?.(e);
        }}
      >
        {show ? on : off}
        {label && <label>{label}</label>}
        {onLabel && show && <label>{onLabel}</label>}
        {offLabel && !show && <label>{offLabel}</label>}
      </div>
    </styled.ToggleButton>
  );
};
