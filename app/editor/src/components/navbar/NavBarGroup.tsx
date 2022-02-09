import * as styled from './NavBarStyled';

export interface INaVBarGroupProps {
  /**
   * choose a background colour for the navigation bar
   */
  backgroundColor?: string;
  /**
   * place desired nodes within the navigation bar group
   */
  children?: React.ReactNode;
}

export const NavBarGroup: React.FC<INaVBarGroupProps> = ({ children }) => {
  return <styled.NavBarGroup>{children}</styled.NavBarGroup>;
};
