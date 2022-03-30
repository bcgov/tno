import { InputHTMLAttributes } from 'react';
import React from 'react';

import { TextVariant } from '..';
import { FieldSize } from '../constants';
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
  const [errorMsg, setErrorMsg] = React.useState(error);

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
        role={errorMsg ? 'alert' : 'none'}
        onInput={(e) => {
          if (onInput) onInput(e);
          else {
            const input = e.target as HTMLInputElement;
            input.setCustomValidity('');
            setErrorMsg(undefined);
          }
        }}
        onInvalid={(e) => {
          if (onInvalid) return onInvalid(e);
          else {
            const input = e.target as HTMLInputElement;
            if (rest.required && input.validity.valueMissing) {
              input.setCustomValidity(error ?? 'required');
              setErrorMsg(error ?? 'required');
            }
          }
        }}
        {...rest}
      >
        {children}
      </styled.TextAreaField>
      {errorMsg && <p role="alert">{errorMsg}</p>}
    </styled.TextArea>
  );
};
