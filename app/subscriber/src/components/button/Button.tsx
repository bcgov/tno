import * as styled from './styled';

export interface IButtonProps extends React.HTMLAttributes<HTMLButtonElement> {}

/**
 * Provides a common styled button.
 * @param param0 Component properties
 * @returns Component
 */
export const Button: React.FC<IButtonProps> = ({ children, ...rest }) => {
  return <styled.Button {...rest}>{children}</styled.Button>;
};
