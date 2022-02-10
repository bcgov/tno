import { NavBarGroup, NavBarItem, Row } from 'components';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  return (
    <NavBarGroup>
      <Row style={{ marginLeft: '5em' }}>
        <NavBarItem navigateTo="/contents" label="Snippets" />
        <NavBarItem navigateTo="/admin" label="Admin" />
      </Row>
    </NavBarGroup>
  );
};
