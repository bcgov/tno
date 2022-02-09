import * as styled from './NavBarStyled';

export interface INavBarGroupProps {
  /**
   * place desired nodes within the navigation bar group
   */
  children?: React.ReactNode;
}

/**
 * The element that groups the various navigation bar items together.
 * @param children the navigation bar items
 * @returns navigation bar group
 */
export const NavBarGroup: React.FC<INavBarGroupProps> = ({ children }) => {
  return <styled.NavBarGroup>{children}</styled.NavBarGroup>;
};
