import { InputHTMLAttributes } from 'react';
import React from 'react';
import { FieldSize, TextVariant } from 'tno-core';

import * as styled from './styled';

export interface ITextAreaProps extends InputHTMLAttributes<HTMLTextAreaElement> {
  /**
   * Form field name and field assessor.
   */
  name: string;
  /**
   * Form field label.
   */
  label?: string;
  /**
   * The styled variant.
   */
  variant?: TextVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
  /**
   * Form field size.
   */
  width?: FieldSize;
  /**
   * Error message.
   */
  error?: string;
}

export const TextArea: React.FC<ITextAreaProps> = ({
  id,
  name,
  label,
  variant = TextVariant.primary,
  tooltip,
  children,
  className,
  error,
  width,
  onInput,
  onInvalid,
  ...rest
}) => {
  return (
    <styled.TextArea className="frm-in">
      {label && (
        <label className={rest.required ? 'required' : ''} htmlFor={id ?? `txa-${name}`}>
          {label}
        </label>
      )}
      <styled.TextAreaField
        id={id}
        name={name}
        variant={variant}
        className={`txa ${className ?? ''}`}
        data-for="main-tooltip"
        data-tip={tooltip}
        width={width}
        role={error ? 'alert' : 'none'}
        onInput={(e) => {
          if (onInput) onInput(e);
          else {
            const input = e.target as HTMLInputElement;
            input.setCustomValidity('');
          }
        }}
        onInvalid={(e) => {
          if (onInvalid) return onInvalid(e);
          else {
            const input = e.target as HTMLInputElement;
            if (rest.required && input.validity.valueMissing) {
              input.setCustomValidity(error ?? 'required');
            }
          }
        }}
        {...rest}
      >
        {children}
      </styled.TextAreaField>
      {error && <p role="alert">{error}</p>}
    </styled.TextArea>
  );
};
