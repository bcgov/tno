import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { Error } from 'components/form';
import React, { Ref } from 'react';
import { ActionMeta, GroupBase, Props } from 'react-select';
import ReactSelect from 'react-select/dist/declarations/src/Select';
import { FieldSize } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

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
  variant?: SelectVariant;
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
  width?: FieldSize;
  /**
   * Error message to display if validation fails.
   */
  error?: string;
  /**
   * When the user pressed the delete button.
   */
  onClear?: () => void;
}

export type SelectProps = ISelectBaseProps &
  Props &
  Omit<React.HTMLAttributes<HTMLSelectElement>, 'defaultValue' | 'onChange'>;

export interface ISelectProps<OptionType> extends SelectProps {
  ref?: Ref<ReactSelect<OptionType, boolean, GroupBase<OptionType>>>;
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
  onClear,
  ...rest
}: ISelectProps<OptionType>) => {
  const selectRef = React.useRef(null);
  const inputRef = React.useRef<HTMLInputElement>(null);
  return (
    <styled.Select className="frm-in">
      {label && (
        <div title={tooltip}>
          <label className={required ? 'required' : ''} htmlFor={`sel-${name}`}>
            {label}
          </label>
          {tooltip && <FontAwesomeIcon icon={faInfoCircle} />}
        </div>
      )}
      <Row
        onKeyUp={(e) => {
          if (e.code === 'Delete') {
            onClear?.();
          }
        }}
        data-for="select-tooltip"
      >
        <styled.SelectField
          ref={selectRef}
          id={id ?? `sel-${name}`}
          name={name}
          className={`${className ?? ''}${!!error ? ' alert' : ''}`}
          classNamePrefix={classNamePrefix ?? 'rs'}
          variant={variant}
          required={required}
          width={width}
          value={value}
          options={options}
          onChange={(newValue: unknown, actionMeta: ActionMeta<unknown>) => {
            onChange?.(newValue, actionMeta);
            inputRef?.current?.setCustomValidity('');
          }}
          onFocus={(e: any) => {
            const input = e.target as HTMLSelectElement;
            input?.setCustomValidity('');
          }}
          {...rest}
        />
        {children}
      </Row>
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
