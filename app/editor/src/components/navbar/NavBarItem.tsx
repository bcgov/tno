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

/**
 * The individual item that will appear in the navigation bar, on click it will navigate to desired path and will use the applications
 * current path to determine whether it is active or not.
 * @param label the text to appear on the tab in the navigation bar
 * @param navigateTo determine the path the item will navigate to onClick
 * @returns styled navigation bar item
 */
export const NavBarItem: React.FC<INavBarItemProps> = ({ label, navigateTo }) => {
  let path = window.location.pathname;
  let isActive = navigateTo ? path.includes(navigateTo) : false;
  const navigate = useNavigate();
  return (
    <styled.NavBarItem onClick={() => navigate(navigateTo!!)} active={isActive}>
      {label}
    </styled.NavBarItem>
  );
};
