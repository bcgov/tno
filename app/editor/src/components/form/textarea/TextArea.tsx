import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Error } from 'components/form';
import { InputHTMLAttributes } from 'react';
import React from 'react';
import { FieldSize, Show, TextVariant } from 'tno-core';

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
      <Show visible={!!label}>
        <label
          data-for="main-tooltip"
          data-tip={tooltip}
          className={rest.required ? 'required' : ''}
          htmlFor={id ?? `txa-${name}`}
        >
          {label}
          {tooltip && <FontAwesomeIcon icon={faInfoCircle} />}
        </label>
      </Show>
      <styled.TextAreaField
        id={id}
        name={name}
        variant={variant}
        className={`txa ${className ?? ''}`}
        data-for="main-tooltip"
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
      <Error error={error} />
    </styled.TextArea>
  );
};
