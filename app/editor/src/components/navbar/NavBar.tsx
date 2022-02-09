import { NavBarGroup, NavBarItem, Row } from 'components';

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
