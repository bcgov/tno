import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import React, { InputHTMLAttributes } from 'react';

import { Error } from '../../form';
import { instanceOfIOption, IOptionItem } from '..';
import { Radio, RadioVariant } from '.';
import * as styled from './styled';

export interface IRadioGroupProps<OT extends string | number | IOptionItem | HTMLOptionElement>
  extends Omit<InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange'> {
  /**
   * The control name.
   */
  name: string;
  /**
   * Whether to include space under the radio.  // TODO: replace property with appropriate styling options.
   */
  spaceUnderRadio?: boolean;
  /**
   * The label to include with the control.
   */
  label?: string;
  /**
   * The styled variant.
   */
  variant?: RadioVariant;
  /**
   * Tooltip to display.
   */
  tooltip?: string;
  /**
   * An array of options.
   */
  options?: readonly OT[];
  /**
   * The current value.
   */
  value?: OT;
  /**
   * Flex flow direction.
   */
  direction?: 'row' | 'column' | 'col-row';
  /**
   * An error message.
   */
  error?: string;
  /**
   * OnChange event
   */
  onChange?: (event: React.ChangeEvent<HTMLInputElement>, values: OT | undefined) => void;
}

/**
 * RadioGroup component provides a bootstrapped styled input radio element.
 * @param param0 RadioGroup element attributes.
 * @returns RadioGroup component.
 */
export const RadioGroup = <OT extends string | number | IOptionItem | HTMLOptionElement>({
  name,
  label,
  tooltip,
  children,
  options,
  value,
  className,
  spaceUnderRadio,
  error,
  direction = 'column',
  onChange,
  ...rest
}: IRadioGroupProps<OT>) => {
  const [selected, setSelected] = React.useState<OT | undefined>(value);

  /** for when the value needs to be passed down on initial load */
  React.useEffect(() => {
    setSelected(value);
  }, [value]);

  const handleChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const value = e.target.value;
    const isChecked = e.target.checked;

    if (isChecked) {
      const found: OT = (options as any)?.find((option: any) => {
        if (typeof option === 'string') return option === value;
        if (typeof option === 'number') return option === +value;
        if (instanceOfIOption(option)) return `${option.value}` === value;
        return option === value;
      });
      if (found) setSelected(found);
      onChange?.(e, found);
    } else {
      setSelected(undefined);
      onChange?.(e, undefined);
    }
  };

  return (
    <styled.RadioGroup
      name={name}
      className={`frm-in rag${className ? ` ${className}` : ''}`}
      direction={direction}
      {...rest}
    >
      <div>
        {label && (
          <label
            data-tooltip-id="main-tooltip"
            data-tooltip-content={tooltip}
            className={rest.required ? 'required' : ''}
          >
            {label} {tooltip && <FontAwesomeIcon icon={faInfoCircle} />}
          </label>
        )}
        {options
          ? options.map((option) => {
              if (instanceOfIOption(option)) {
                const item = option as IOptionItem;
                return (
                  <styled.Span spaceUnderRadio={!!spaceUnderRadio} key={item.value}>
                    <Radio
                      id={`rad-${name}-${item.value}`}
                      name={name}
                      error={error}
                      value={item.value}
                      checked={item.value === (selected as IOptionItem)?.value}
                      onChange={handleChange}
                      {...rest}
                    />
                    <label htmlFor={`rad-${name}-${item.value}`}>{item.label}</label>
                  </styled.Span>
                );
              } else if (typeof option === 'object') {
                // TODO: Validate option is HTMLOptionElement
                const element = option as unknown as React.ReactNode;
                return element;
              } else if (typeof option === 'number') {
                const value = option as number;
                return (
                  <styled.Span spaceUnderRadio={!!spaceUnderRadio} key={value}>
                    <Radio
                      id={`rad-${name}-${value}`}
                      name={name}
                      value={value}
                      checked={value === selected}
                      error={error}
                      {...rest}
                      onChange={handleChange}
                    />
                    <label htmlFor={`rad-${name}-${value}`}>{value}</label>
                  </styled.Span>
                );
              } else {
                const value = option as string;

                return (
                  <styled.Span spaceUnderRadio={!!spaceUnderRadio} key={value}>
                    <Radio
                      error={error}
                      id={`rad-${name}-${value}`}
                      name={name}
                      value={value}
                      checked={value === selected}
                      {...rest}
                      onChange={handleChange}
                    />
                    <label htmlFor={`rad-${name}-${value}`}>{value}</label>
                  </styled.Span>
                );
              }
            })
          : children}
      </div>
      <Error error={error} />
    </styled.RadioGroup>
  );
};
