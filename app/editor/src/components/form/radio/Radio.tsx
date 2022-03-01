import React, { InputHTMLAttributes } from 'react';

import { RadioVariant } from '.';
import * as styled from './styled';

export interface IRadioProps extends InputHTMLAttributes<HTMLInputElement> {
  /**
   * The styled variant.
   */
  variant?: RadioVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
}

/**
 * Radio component provides a bootstrapped styled input radio element.
 * @param param0 Radio element attributes.
 * @returns Radio component.
 */
export const Radio: React.FC<IRadioProps> = ({
  type = 'radio',
  variant = RadioVariant.primary,
  tooltip,
  children,
  className,
  ...rest
}) => {
  return (
    <styled.Radio
      type={type}
      variant={variant}
      className={`rad ${className ?? ''}`}
      data-for="main-tooltip"
      data-tip={tooltip}
      {...rest}
    >
      {children}
    </styled.Radio>
  );
};
