import React from 'react';

import * as styled from './styled';

type IToggleOption = {
  label: string;
  onClick?: () => void;
};

export interface IToggleGroupProps {
  options?: IToggleOption[];
}

/**
 * A group of toggle buttons that can be used to perform actions
 * @param options The options to display in the toggle group, can contain a label and an onClick function
 * @returns The ToggleGroup component
 */
export const ToggleGroup: React.FC<IToggleGroupProps> = ({ options }) => {
  const [activeToggle, setActiveToggle] = React.useState('');
  return (
    <styled.ToggleGroup>
      {options?.map((option) => (
        <button
          className={`toggle-item ${activeToggle === option.label ? 'active' : ''}`}
          onClick={() => {
            setActiveToggle(option.label);
            option.onClick?.();
          }}
        >
          {option.label}
        </button>
      ))}
    </styled.ToggleGroup>
  );
};
