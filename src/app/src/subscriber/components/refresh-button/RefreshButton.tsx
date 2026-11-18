import React, { MouseEventHandler } from 'react';

import * as styled from './styled';

export interface IRefreshButtonProps {
  label?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  title?: string;
  icon: any;
  disabled?: boolean;
  direction?: any;
}
export const RefreshButton: React.FC<IRefreshButtonProps> = ({
  label,
  onClick,
  icon,
  title,
  disabled,
  direction,
}) => {
  return (
    <styled.RefreshButton
      onClick={onClick}
      title={title}
      icon={icon}
      disabled={disabled}
      label={label}
      direction={direction}
    />
  );
};
