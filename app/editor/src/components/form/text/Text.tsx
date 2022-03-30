import { Row } from 'components/flex';
import React, { InputHTMLAttributes } from 'react';

import { FieldSize } from '../constants';
import { TextVariant } from '.';
import * as styled from './styled';

export interface ITextProps extends InputHTMLAttributes<HTMLInputElement> {
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
   * Error message.
   */
  error?: string;
  /**
   * Form field size.
   */
  width?: FieldSize;
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
  error,
  width = FieldSize.Stretch,
  onInput,
  onInvalid,
  ...rest
}) => {
  const [errorMsg, setErrorMsg] = React.useState(error);

  return (
    <styled.Text className="frm-in">
      {label && (
        <label className={rest.required ? 'required' : ''} htmlFor={id ?? `txt-${name}`}>
          {label}
        </label>
      )}
      <Row>
        <styled.TextField
          name={name}
          id={id}
          type={type}
          variant={variant}
          className={`txt ${className ?? ''}`}
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
        />
        {children}
      </Row>
      {errorMsg && <p role="alert">{errorMsg}</p>}
    </styled.Text>
  );
};
