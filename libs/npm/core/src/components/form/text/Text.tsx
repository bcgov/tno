import React, { InputHTMLAttributes } from 'react';
import { FaInfoCircle } from 'react-icons/fa';

import { Row } from '../../flex';
import { Error } from '..';
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
  width?: FieldSize | string;
  /**
   * Function to format the value.
   */
  formatter?: (
    value: string | number | readonly string[] | undefined,
  ) => string | number | readonly string[] | undefined;
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
  value,
  formatter = (value) => value,
  onInput,
  onInvalid,
  ...rest
}) => {
  return (
    <styled.Text className={`frm-in${className ? ` ${className}` : ''}`}>
      {label && (
        <label
          data-tooltip-id="main-tooltip"
          data-tooltip-content={tooltip}
          className={rest.required ? 'required' : ''}
          htmlFor={id ?? `txt-${name}`}
        >
          {label} {tooltip && <FaInfoCircle />}
        </label>
      )}
      <Row nowrap>
        <styled.TextField
          name={name}
          id={id}
          type={type}
          variant={variant}
          className="txt"
          data-tooltip-id="main-tooltip"
          width={width}
          role={error ? 'alert' : 'none'}
          value={formatter(value)}
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
        />
        {children}
      </Row>
      <Error error={error} />
    </styled.Text>
  );
};
