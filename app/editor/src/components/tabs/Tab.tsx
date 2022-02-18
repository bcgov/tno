import React, { ButtonHTMLAttributes } from 'react';

import * as styled from './TabStyled';

export interface ITabProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /**
   * Whether the tab is active
   */
  active?: boolean;
  /**
   * The text to display on the tab
   */
  label?: string;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
}

export const Tab: React.FC<ITabProps> = ({
  type = 'button',
  tooltip,
  className,
  children,
  label,
  ...rest
}) => {
  return (
    <styled.Tab
      type={type}
      className={`btn ${className}`}
      data-for="main"
      data-tip={tooltip}
      {...rest}
    >
      {label}
    </styled.Tab>
  );
};
