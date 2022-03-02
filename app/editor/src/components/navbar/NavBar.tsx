import { NavBarGroup, NavBarItem, Row } from 'components';
import { Claim } from 'tno-core';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  return (
    <NavBarGroup>
      <Row>
        <NavBarItem navigateTo="/contents" label="Snippets" />
        <NavBarItem navigateTo="/admin" label="Admin" claim={Claim.administrator} />
      </Row>
    </NavBarGroup>
  );
};
