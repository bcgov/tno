import React from 'react';
import { FaAngleDown } from 'react-icons/fa';
import { Col, IOptionItem, IRowProps, Row, Show } from 'tno-core';

import * as styled from './styled';

interface IToggleOption {
  id?: string | number;
  label: string;
  dropDownOptions?: IOptionItem[];
  onClick?: (value?: number) => void;
}

export interface IToggleGroupProps extends IRowProps {
  label?: string;
  options: IToggleOption[];
  defaultSelected?: string | number;
  disabled?: boolean;
}

/**
 * A group of toggle buttons that can be used to perform actions
 * @param options The options to display in the toggle group, can contain a label and an onClick function
 * @param defaultSelected The default selected option
 * @param disabled Whether the toggle group is disabled
 * @returns The ToggleGroup component
 */
export const ToggleGroup: React.FC<IToggleGroupProps> = ({
  label,
  options,
  defaultSelected = '',
  disabled,
  ...rest
}) => {
  const ref = React.useRef<HTMLDivElement>(null);

  const [activeToggle, setActiveToggle] = React.useState(defaultSelected);
  const [showDropDown, setShowDropDown] = React.useState(false);

  // ensure default selected gets reset when new content is loaded
  React.useEffect(() => {
    if (defaultSelected) setActiveToggle(defaultSelected);
    if (disabled) setActiveToggle('');
  }, [defaultSelected, disabled]);

  // Close dropdown when clicking outside of it
  React.useEffect(() => {
    const handleClickOutside = (event: any) => {
      if (ref.current && !ref.current.contains(event.target)) {
        showDropDown && setShowDropDown(false);
      }
    };
    document.addEventListener('click', handleClickOutside, true);
    return () => {
      document.removeEventListener('click', handleClickOutside, true);
    };
  }, [showDropDown]);

  return (
    <styled.ToggleGroup {...rest}>
      {label && <label>{label}</label>}
      {options?.map((option) => (
        <button
          key={option.id ?? option.label}
          disabled={disabled}
          className={`toggle-item ${activeToggle === (option.id ?? option.label) ? 'active' : ''}`}
          type="button"
          onClick={() => {
            setActiveToggle(option.id ?? option.label);
            setShowDropDown(!showDropDown);
            option.onClick?.();
          }}
        >
          <Col>
            <Row>
              {option.label}
              {option.dropDownOptions && <FaAngleDown />}
            </Row>
            <Show visible={!!option.dropDownOptions && showDropDown}>
              <div ref={ref} className="dd-menu">
                <Col>
                  {option.dropDownOptions?.map((x) => (
                    <div
                      key={x.value}
                      className="dd-item"
                      onClick={() => {
                        if (typeof x.value === 'number') option.onClick?.(x.value);
                        setShowDropDown(false);
                      }}
                    >
                      {x.label}
                    </div>
                  ))}
                </Col>
              </div>
            </Show>
          </Col>
        </button>
      ))}
    </styled.ToggleGroup>
  );
};
