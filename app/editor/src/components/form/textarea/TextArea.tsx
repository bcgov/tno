import { InputHTMLAttributes } from 'react';

import { TextVariant } from '..';
import * as styled from './TextAreaStyled';

export interface ITextAreaProps extends InputHTMLAttributes<HTMLTextAreaElement> {
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

export const TextArea: React.FC<ITextAreaProps> = ({
  id,
  name,
  label,
  variant = TextVariant.primary,
  tooltip,
  children,
  className,
  ...rest
}) => {
  return (
    <div className="frm-in">
      {label && <label htmlFor={id ?? `txa-${name}`}>{label}</label>}
      <styled.TextArea
        id={id}
        name={name}
        variant={variant}
        className={`txa ${className ?? ''}`}
        data-for="main-tooltip"
        data-tip={tooltip}
        {...rest}
      >
        {children}
      </styled.TextArea>
    </div>
  );
};
