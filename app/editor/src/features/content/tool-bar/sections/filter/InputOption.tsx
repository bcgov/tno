import React from 'react';
import { components } from 'react-select';

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

  const style = {
    alignItems: 'center',
    backgroundColor: bg,
    color: 'inherit',
    display: 'flex ',
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
      <input type="checkbox" checked={isSelected} />
      {children}
    </components.Option>
  );
};
