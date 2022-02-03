import * as styled from './NavBarStyled';

export interface INaVBarProps {
  backgroundColor?: string;
  children?: React.ReactNode;
}

export const NavBar: React.FC<INaVBarProps> = ({ children }) => {
  return <styled.NavBar>{children}</styled.NavBar>;
};
