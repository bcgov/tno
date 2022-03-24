import React, { InputHTMLAttributes } from 'react';

import { CheckboxVariant } from '.';
import { LabelPosition } from './constants';
import * as styled from './styled';

export interface ICheckboxProps extends InputHTMLAttributes<HTMLInputElement> {
  /**
   * The label to include with the control.
   */
  label?: string;
  /**
   * The styled variant.
   */
  variant?: CheckboxVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
  /**
   * Reference to DOM.
   */
  ref?: any;
  /**
   * Error message.
   */
  error?: string;
  /**
   * Position of label.
   */
  labelPosition?: LabelPosition;
}

/**
 * Checkbox component provides a bootstrapped styled input checkbox element.
 * @param param0 Checkbox element attributes.
 * @returns Checkbox component.
 */
export const Checkbox: React.FC<ICheckboxProps> = ({
  id,
  name,
  label,
  labelPosition = LabelPosition.Right,
  value,
  type = 'checkbox',
  variant = CheckboxVariant.primary,
  tooltip,
  children,
  ref,
  error,
  onInput,
  onInvalid,
  ...rest
}) => {
  const [errorMsg, setErrorMsg] = React.useState(error);

  const labelId = id ?? `${name}-${value}`;
  const LabelInput = <label htmlFor={labelId}>{label}</label>;

  return (
    <styled.Checkbox
      className="frm-in chk"
      data-for="main-tooltip"
      data-tip={tooltip}
      labelPosition={labelPosition}
    >
      <div>
        {label &&
          (labelPosition === LabelPosition.Top || labelPosition === LabelPosition.Left) &&
          LabelInput}
        <styled.CheckboxField
          id={labelId}
          name={name}
          value={value}
          ref={ref}
          type={type}
          variant={variant}
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
        </styled.CheckboxField>
        {label &&
          (labelPosition === LabelPosition.Right || labelPosition === LabelPosition.Bottom) &&
          LabelInput}
      </div>
      {errorMsg && <p role="alert">{errorMsg}</p>}
    </styled.Checkbox>
  );
};
