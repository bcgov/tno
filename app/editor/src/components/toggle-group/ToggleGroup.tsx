import React from 'react';

import * as styled from './styled';

interface IToggleOption {
  label: string;
  onClick?: () => void;
}
export interface IToggleGroupProps {
  options: IToggleOption[];
  defaultSelected?: string;
}

/**
 * A group of toggle buttons that can be used to perform actions
 * @param options The options to display in the toggle group, can contain a label and an onClick function
 * @returns The ToggleGroup component
 */
export const ToggleGroup: React.FC<IToggleGroupProps> = ({ options, defaultSelected }) => {
  const [activeToggle, setActiveToggle] = React.useState(defaultSelected ?? '');
  return (
    <styled.ToggleGroup>
      {options?.map((option) => (
        <button
          className={`toggle-item ${activeToggle === option.label.toLowerCase() ? 'active' : ''}`}
          onClick={() => {
            setActiveToggle(option.label.toLowerCase());
            option.onClick?.();
          }}
        >
          {option.label}
        </button>
      ))}
    </styled.ToggleGroup>
  );
};
