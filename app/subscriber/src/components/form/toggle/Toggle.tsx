import React from 'react';

import * as styled from './styled';

export interface IToggleOption<T> {
  /** Unique id of the toggle option. */
  id?: string;
  /** The value of the toggle option. */
  value: T;
  /** The label of the toggle option. */
  label: React.ReactNode;
  /** An icon for the toggle option. */
  icon?: React.ReactNode;
}

export interface IToggleProps<T> extends Omit<React.HTMLAttributes<HTMLDivElement>, 'onChange'> {
  /** The form name of the toggle input. */
  name: string;
  /** The toggle label. */
  label?: string;
  /** The toggle current value. */
  value?: T;
  /** An array of toggle options. */
  options: IToggleOption<T>[];
  /** Whether this toggle is disabled. */
  disabled?: boolean;
  /** Event fires when the toggle value changes. */
  onChange?: (value: T) => void;
}

/**
 * Provides a toggle component input that displays the provided options.
 * @param param0 Component properties.
 * @returns Component
 */
export const Toggle = <T,>({
  name,
  label,
  options,
  value: initValue,
  className,
  disabled = false,
  onChange,
  ...rest
}: IToggleProps<T>) => {
  const [value, setValue] = React.useState(initValue);

  React.useEffect(() => {
    setValue(initValue);
  }, [initValue]);

  return (
    <styled.Toggle className={`toggle${className ? ` ${className}` : ''}`} {...rest}>
      {label && <label>{label}</label>}
      <div className="options">
        {options.map((option, index) => {
          const valueType = typeof option.value;
          return (
            <div
              className={`option${value === option.value ? ' active' : ''}${
                disabled ? ' disabled' : ''
              }`}
              key={
                option.id ?? (valueType === 'string' || valueType === 'number')
                  ? `${option.value}`
                  : index
              }
              onClick={() => {
                if (!disabled && value !== option.value) {
                  setValue(option.value);
                  onChange?.(option.value);
                }
              }}
            >
              {option.icon}
              <div>{option.label}</div>
            </div>
          );
        })}
      </div>
    </styled.Toggle>
  );
};
