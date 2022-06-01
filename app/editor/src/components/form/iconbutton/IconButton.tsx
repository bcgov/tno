import { ButtonVariant, IButtonProps } from 'tno-core';

import * as styled from './styled';

export interface IIconButtonProps extends IButtonProps {
  /** The text to appear inside the button */
  label?: string;
  /** The icon type */
  iconType: 'plus' | 'search' | 'back' | 'reset';
}

/** Simple button that may contain an icon */
export const IconButton: React.FC<IIconButtonProps> = ({ label, iconType, ...rest }) => {
  return (
    <styled.IconButton
      {...rest}
      label={label}
      iconType={iconType}
      variant={ButtonVariant.secondary}
    >
      <img alt={iconType} src={`${process.env.PUBLIC_URL}/assets/${iconType}.svg`} />
      {label}
    </styled.IconButton>
  );
};
