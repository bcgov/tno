import { IOptionItem } from 'components/form';
import React from 'react';
import { FaAngleDown } from 'react-icons/fa';
import { Col, Row, Show } from 'tno-core';

import * as styled from './styled';

interface IToggleOption {
  label: string;
  dropDownOptions?: IOptionItem[];
  onClick?: (value?: number) => void;
}
export interface IToggleGroupProps {
  options: IToggleOption[];
  defaultSelected?: string;
}

/**
 * A group of toggle buttons that can be used to perform actions
 * @param options The options to display in the toggle group, can contain a label and an onClick function
 * @param defaultSelected The default selected option
 * @returns The ToggleGroup component
 */
export const ToggleGroup: React.FC<IToggleGroupProps> = ({ options, defaultSelected }) => {
  const [activeToggle, setActiveToggle] = React.useState(defaultSelected ?? '');
  const [showDropDown, setShowDropDown] = React.useState(false);
  const onDropDownClick = () => {
    setShowDropDown(!showDropDown);
  };
  const ref = React.useRef<HTMLDivElement>(null);

  // ensure default selected gets reset when new content is loaded
  React.useEffect(() => {
    if (defaultSelected) setActiveToggle(defaultSelected);
  }, [defaultSelected]);

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
    <styled.ToggleGroup>
      {options?.map((option) => (
        <button
          key={option.label}
          className={`toggle-item ${activeToggle === option.label.toLowerCase() ? 'active' : ''}`}
          type="button"
          onClick={() => {
            setActiveToggle(option.label.toLowerCase());
            !!option.dropDownOptions && onDropDownClick();
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
                        option.onClick && typeof x.value === 'number' && option.onClick(x.value);
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
