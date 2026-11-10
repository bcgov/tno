import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { debounce } from 'lodash';
import { InputHTMLAttributes } from 'react';
import React from 'react';

import { Error, FieldSize, TextVariant } from '../../form';
import { Show } from '../../show';
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
  width?: FieldSize | string;
  /**
   * Error message.
   */
  error?: string;
  /**
   * The number of rows to show.
   */
  rows?: number;
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
  rows,
  ...rest
}) => {
  const handleInput = debounce((e) => {
    const input = e.target as HTMLInputElement;
    input.setCustomValidity('');
  }, 500);

  const handleInvalid = debounce((e) => {
    const input = e.target as HTMLInputElement;
    if (rest.required && input.validity.valueMissing) {
      input.setCustomValidity(error ?? 'required');
    }
  }, 500);

  return (
    <styled.TextArea className="frm-in">
      <Show visible={!!label}>
        <label
          data-tooltip-id="main-tooltip"
          data-tooltip-content={tooltip}
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
        data-tooltip-id="main-tooltip"
        width={width}
        role={error ? 'alert' : 'none'}
        rows={rows}
        onInput={(e) => {
          if (onInput) onInput(e);
          else handleInput(e);
        }}
        onInvalid={(e) => {
          if (onInvalid) return onInvalid(e);
          else handleInvalid(e);
        }}
        {...rest}
      >
        {children}
      </styled.TextAreaField>
      <Error error={error} />
    </styled.TextArea>
  );
};
