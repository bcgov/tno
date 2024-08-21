import React from 'react';
import { FaAngleDown } from 'react-icons/fa';

import { Col, IRowProps, Row } from '../flex';
import { IOptionItem } from '../form';
import { Show } from '../show';
import * as styled from './styled';

export interface IToggleOption {
  id?: string | number;
  /** the label for the toggle button or menu, will be used to identify default selected if id is not set */
  label: React.ReactNode;
  /** An icon to display */
  icon?: React.ReactNode;
  dropDownOptions?: IOptionItem[];
  /** Event fires when option is clicked. */
  onClick?: (
    event?: React.MouseEvent<HTMLButtonElement | HTMLDivElement, MouseEvent>,
    subValue?: IOptionItem,
  ) => void;
}

export interface IToggleGroupProps extends Omit<IRowProps, 'onChange'> {
  label?: React.ReactNode;
  options: IToggleOption[];
  defaultSelected?: string | number;
  disabled?: boolean;
  activeColor?: string;
  /** Event fires when option is clicked. */
  onChange?: (newValue?: IToggleOption, subValue?: IOptionItem) => void;
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
  onChange,
  ...rest
}) => {
  const ref = React.useRef<HTMLDivElement>(null);

  const [activeToggle, setActiveToggle] = React.useState(defaultSelected);
  const [showDropDown, setShowDropDown] = React.useState(false);

  // ensure default selected gets reset when new content is loaded
  React.useEffect(() => {
    if (disabled) setActiveToggle('');
    else setActiveToggle(defaultSelected);
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
      {options?.map((option, index) => (
        <button
          key={option.id ?? (typeof option.label === 'string' ? option.label : `option-${index}`)}
          disabled={disabled}
          className={`toggle-item ${activeToggle === (option.id ?? option.label) ? 'active' : ''}`}
          type="button"
          onClick={(e) => {
            setActiveToggle(
              option.id ?? (typeof option.label === 'string' ? option.label : `option-${index}`),
            );
            if (option.label === 'OTHER') setShowDropDown(!showDropDown);
            option.onClick?.(e);
            onChange?.(option);
          }}
        >
          <Col>
            <Row>
              {option.icon}
              {option.label}
              {option.dropDownOptions && <FaAngleDown />}
            </Row>
            <Show visible={!!option.dropDownOptions && showDropDown}>
              <div ref={ref} className="dd-menu">
                <Col>
                  {option.dropDownOptions?.map((item) => (
                    <div
                      key={item.value}
                      className="dd-item"
                      onClick={(e) => {
                        if (typeof item.value === 'number') {
                          option.onClick?.(e, item);
                          onChange?.(option, item);
                        }
                        setShowDropDown(false);
                      }}
                    >
                      {item.label}
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
