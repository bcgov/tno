import * as styled from './styled';

export interface IButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {}

/**
 * Provides a common styled button.
 * @param param0 Component properties
 * @returns Component
 */
export const Button: React.FC<IButtonProps> = ({ onClick, type = 'button', children, ...rest }) => {
  return (
    <styled.Button
      type={type}
      {...rest}
      onClick={(e) => {
        if (!rest.disabled) onClick?.(e);
      }}
    >
      {children}
    </styled.Button>
  );
};
