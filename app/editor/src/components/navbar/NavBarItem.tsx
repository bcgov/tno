import * as styled from './NavBarItemStyled';

export interface INavBarItemProps extends React.HTMLProps<HTMLButtonElement> {
  children?: React.ReactNode;
  active?: boolean;
}

export const NavBarItem: React.FC<INavBarItemProps> = ({ children, active }) => {
  return <styled.NavBarItem>{children}</styled.NavBarItem>;
};
