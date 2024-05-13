import React from 'react';

import * as styled from './styled';

export interface IToggleButtonProps {
  on: React.ReactNode;
  off: React.ReactNode;
  value?: boolean;
  onClick?: React.MouseEventHandler<HTMLDivElement>;
}

export const ToggleButton = ({ on, off, value, onClick }: IToggleButtonProps) => {
  const [show, setShow] = React.useState(value);

  React.useEffect(() => {
    setShow(value);
  }, [value]);

  return (
    <styled.ToggleButton
      onClick={(e) => {
        setShow(!show);
        onClick?.(e);
      }}
    >
      {show ? on : off}
    </styled.ToggleButton>
  );
};
