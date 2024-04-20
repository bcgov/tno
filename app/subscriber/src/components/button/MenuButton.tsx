import * as styled from './styled';

export interface IMenuButtonProps {
  label?: React.ReactNode;
  active?: boolean;
  disabled?: boolean;
  className?: string;
  children?: React.ReactNode;
  onClick?: React.MouseEventHandler<HTMLDivElement>;
}

export const MenuButton = ({
  label,
  active = false,
  className,
  disabled,
  children,
  onClick,
}: IMenuButtonProps) => {
  return (
    <styled.MenuButton
      className={`btn-menu${className ? ` ${className}` : ''}`}
      active={active}
      disabled={disabled}
      onClick={onClick}
    >
      {label}
      {children}
    </styled.MenuButton>
  );
};
