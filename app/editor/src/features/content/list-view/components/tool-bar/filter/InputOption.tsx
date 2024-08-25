import React from 'react';
import { components } from 'react-select';

import * as styled from './styled';

/** Custom option for react-select component adding a checkbox and label as the selection item */
export const InputOption: React.FC<any> = ({
  getStyles,
  Icon,
  isDisabled,
  isFocused,
  isSelected,
  children,
  innerProps,
  ...rest
}) => {
  const [isActive, setIsActive] = React.useState(false);
  const onMouseDown = () => setIsActive(true);
  const onMouseUp = () => setIsActive(false);
  const onMouseLeave = () => setIsActive(false);

  // unique use case for this component - avoid styled components to comply with react-select
  let bg = 'transparent';
  if (isFocused) bg = '#eee';
  if (isActive) bg = '#B2D4FF';
  if (isSelected) bg = '#2293e9';

  const style = {
    alignItems: 'center',
    color: isSelected ? 'white' : 'inherit',
    display: 'flex ',
    backgroundColor: bg,
  };

  const props = {
    ...innerProps,
    onMouseDown,
    onMouseUp,
    onMouseLeave,
    style,
  };

  return (
    <components.Option
      {...rest}
      isDisabled={isDisabled}
      isFocused={isFocused}
      isSelected={isSelected}
      getStyles={getStyles}
      innerProps={props}
    >
      <input type="checkbox" id={children} defaultChecked={isSelected} />
      <styled.SelectLabel
        className="label"
        onClick={() => children && document.getElementById(children)?.click()}
      >
        {children}
      </styled.SelectLabel>
    </components.Option>
  );
};
