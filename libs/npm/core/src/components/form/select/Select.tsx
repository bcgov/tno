import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React from 'react';
import { ActionMeta, GroupBase, Props } from 'react-select';
import ReactSelect from 'react-select/dist/declarations/src/Select';

import { Row } from '../../flex';
import { Error, FieldSize } from '../../form';
import { IOptionItem } from '..';
import { SelectVariant } from '.';
import * as styled from './styled';

export interface ISelectBaseProps {
  /**
   * Name of the form field.
   */
  name: string;
  /**
   * The label to include with the control.
   */
  label?: string;
  /**
   * The styled variant.
   */
  variant?: SelectVariant | string;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
  /**
   * Whether this form field is required.
   */
  required?: boolean;
  /**
   * Size of field.
   */
  width?: FieldSize | string;
  /**
   * Error message to display if validation fails.
   */
  error?: string;
  /**
   * The value to set when clearing the current selection.
   */
  clearValue?: unknown;
  /**
   * Key down capture event.  The react-select component doesn't natively support, so this is a workaround.
   */
  onKeyDownCapture?: (e: React.KeyboardEvent<HTMLDivElement>) => void;
  /**
   * Key up press event.  The react-select component doesn't natively support, so this is a workaround.
   */
  onKeyUp?: (e: React.KeyboardEvent<HTMLDivElement>) => void;
  /**
   * Key up capture event.  The react-select component doesn't natively support, so this is a workaround.
   */
  onKeyUpCapture?: (e: React.KeyboardEvent<HTMLDivElement>) => void;
}

export type SelectProps = ISelectBaseProps &
  Props &
  Omit<
    React.HTMLAttributes<HTMLSelectElement>,
    'defaultValue' | 'onChange' | 'onKeyDownCapture' | 'onKeyUp' | 'onKeyUpCapture'
  >;

export interface ISelectProps<OptionType> extends SelectProps {
  ref?: React.Ref<ReactSelect<OptionType, boolean, GroupBase<OptionType>>>;
}

/**
 * Select component provides a bootstrapped styled button element.
 * @param param0 Select element attributes.
 * @returns Select component.
 */
export const Select = <OptionType extends IOptionItem>({
  id,
  ref,
  name,
  label,
  value,
  variant = SelectVariant.primary,
  tooltip,
  children,
  required,
  width = FieldSize.Stretch,
  error,
  classNamePrefix,
  className,
  options,
  onInput,
  onInvalid,
  onChange,
  onKeyDown,
  onKeyDownCapture,
  onKeyUp,
  onKeyUpCapture,
  onBlur,
  ...rest
}: ISelectProps<OptionType>) => {
  const selectRef = React.useRef<any>(null);
  const inputRef = React.useRef<HTMLInputElement>(null);

  return (
    <styled.Select className="frm-in">
      {label && (
        <label
          data-tooltip-id="main-tooltip"
          data-tooltip-content={tooltip}
          className={required ? 'required' : ''}
          htmlFor={`sel-${name}`}
        >
          {label} {tooltip && <FontAwesomeIcon icon={faInfoCircle} />}
        </label>
      )}
      <div
        onKeyDown={onKeyDown}
        onKeyDownCapture={onKeyDownCapture}
        onKeyUp={onKeyUp}
        onKeyUpCapture={onKeyUpCapture}
      >
        <Row
          onKeyUp={(e) => {
            if (e.code === 'Delete') {
              selectRef.current?.clearValue();
            }
          }}
          data-tooltip-id="select-tooltip"
        >
          <styled.SelectField
            ref={selectRef}
            id={id ?? `sel-${name}`}
            name={name}
            className={`${className ?? 'frm-select'}${!!error ? ' alert' : ''}`}
            classNamePrefix={classNamePrefix ?? 'rs'}
            menuPortalTarget={document.body}
            styles={{ menuPortal: (base) => ({ ...base, zIndex: 9999 }) }}
            variant={variant}
            required={required}
            width={width}
            value={value}
            isClearable
            options={options}
            onChange={(newValue: unknown, actionMeta: ActionMeta<unknown>) => {
              if (newValue === null) onChange?.(rest.clearValue, actionMeta);
              else onChange?.(newValue, actionMeta);
              inputRef?.current?.setCustomValidity('');
            }}
            onFocus={(e: any) => {
              const input = e.target as HTMLSelectElement;
              input?.setCustomValidity('');
            }}
            onBlur={(e: any) => {
              onBlur?.(e);
            }}
            {...rest}
          />
          {children}
        </Row>
      </div>
      {!rest.isDisabled && (
        <input
          ref={inputRef}
          tabIndex={-1}
          autoComplete="off"
          style={{
            opacity: 0,
            width: '100%',
            height: 0,
            position: 'absolute',
          }}
          value={(value as OptionType)?.value ?? ''}
          onChange={() => {}}
          onFocus={() => (selectRef.current as any)?.focus()}
          required={required}
          onInput={(e) => {
            const input = e.target as HTMLInputElement;
            input?.setCustomValidity('');
          }}
          onInvalid={(e) => {
            const input = e.target as HTMLInputElement;
            if (required && input?.validity.valueMissing) {
              input.setCustomValidity(error ?? 'required');
            }
          }}
        />
      )}
      <Error error={error} />
    </styled.Select>
  );
};
