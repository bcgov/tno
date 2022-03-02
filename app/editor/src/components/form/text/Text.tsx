import React, { InputHTMLAttributes } from 'react';

import { TextVariant } from '.';
import * as styled from './styled';

export interface ITextProps extends InputHTMLAttributes<HTMLInputElement> {
  name: string;
  label?: string;
  /**
   * The styled variant.
   */
  variant?: TextVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
}

/**
 * Text component provides a bootstrapped styled button element.
 * @param param0 Text element attributes.
 * @returns Text component.
 */
export const Text: React.FC<ITextProps> = ({
  id,
  name,
  label,
  type = 'text',
  variant = TextVariant.primary,
  tooltip,
  children,
  className,
  ...rest
}) => {
  return (
    <div className="frm-in">
      {label && <label htmlFor={id ?? `txt-${name}`}>{label}</label>}
      <styled.Text
        name={name}
        id={id}
        type={type}
        variant={variant}
        className={`txt ${className ?? ''}`}
        data-for="main-tooltip"
        data-tip={tooltip}
        {...rest}
      >
        {children}
      </styled.Text>
    </div>
  );
};
