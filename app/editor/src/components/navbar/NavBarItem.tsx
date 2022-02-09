import { useNavigate } from 'react-router';
import * as styled from './NavBarItemStyled';

export interface INavBarItemProps extends React.HTMLProps<HTMLButtonElement> {
  /**
   * choose the tab label
   */
  label?: string;
  /**
   * prop used to determine whether the tab is active
   */
  active?: boolean;
  /**
   * the path the item will navigate you to
   */
  navigateTo?: string;
}

export const NavBarItem: React.FC<INavBarItemProps> = ({ children, label, navigateTo }) => {
  let path = window.location.pathname;
  let isActive = navigateTo ? path.includes(navigateTo) : false;
  const navigate = useNavigate();
  return <styled.NavBarItem onClick={()=>navigate(navigateTo!!)} active={isActive}>{label}</styled.NavBarItem>;
};
