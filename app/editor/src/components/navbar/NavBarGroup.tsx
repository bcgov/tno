import * as styled from './NavBarStyled';

export interface INavBarGroupProps {
  /**
   * place desired nodes within the navigation bar group
   */
  children?: React.ReactNode;
}

export const NavBarGroup: React.FC<INavBarGroupProps> = ({ children }) => {
  return <styled.NavBarGroup>{children}</styled.NavBarGroup>;
};
