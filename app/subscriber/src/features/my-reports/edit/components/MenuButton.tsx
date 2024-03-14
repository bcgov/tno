import * as styled from './styled';

export interface IMenuButtonProps {
  label?: string;
  active?: boolean;
  disabled?: boolean;
  className?: string;
  onClick?: React.MouseEventHandler<HTMLDivElement>;
}

export const MenuButton = ({
  label,
  active = false,
  className,
  disabled,
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
    </styled.MenuButton>
  );
};
