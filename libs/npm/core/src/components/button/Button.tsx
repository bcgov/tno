import React, { ButtonHTMLAttributes } from 'react';
import { Tooltip } from 'react-tooltip';

import { BouncingSpinner } from '../spinners';
import { ButtonVariant } from '.';
import * as styled from './styled';

export interface IButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /**
   * The styled variant.
   */
  variant?: ButtonVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
  /**
   * Whether the button should show a loading indicator.
   */
  loading?: boolean;
  /**
   * Whether the button should contain rounded edges
   */
  rounded?: boolean;
}

/**
 * Button component provides a bootstrapped styled button element.
 * @param param0 Button element attributes.
 * @returns Button component.
 */
export const Button: React.FC<IButtonProps> = ({
  type = 'button',
  variant = ButtonVariant.primary,
  tooltip,
  className,
  children,
  loading = false,
  ...rest
}) => {
  const onClickHandler = (event: React.MouseEvent<HTMLButtonElement>) => {
    rest.onClick && rest.onClick(event);
  };
  // Generate a unique id for the tooltip - needed for cases of multiple button usage on the same page
  const [uniqueId] = React.useState(new Date().getTime().toString(36));
  return (
    <styled.Button
      type={type}
      variant={variant}
      onClick={onClickHandler}
      className={`btn ${className ?? ''}`}
      data-tooltip-id={`btn-tip-${uniqueId}`}
      data-tooltip-content={tooltip}
      {...rest}
    >
      <div>
        {children}
        {loading && <BouncingSpinner />}
      </div>
      <Tooltip
        style={{ zIndex: '999' }}
        variant="info"
        id={`btn-tip-${uniqueId}`}
        place="top"
        float
      />
    </styled.Button>
  );
};
