import { Row } from 'components/flex/row';
import { NavBarGroup, NavBarItem } from 'components/navbar';
import { useLocation } from 'react-router-dom';
import { Claim } from 'tno-core';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const location = useLocation();

  const showEditor = location.pathname.startsWith('/contents');
  const showAdmin = location.pathname.startsWith('/admin');

  return (
    <NavBarGroup>
      <Row>
        <NavBarItem navigateTo="/contents" label="Editor" claim={Claim.editor} />
        <NavBarItem navigateTo="/admin" label="Admin" claim={Claim.administrator} />
      </Row>
      {showEditor && (
        <Row>
          <NavBarItem navigateTo="/contents" label="Snippets" claim={Claim.editor} />
        </Row>
      )}
      {showAdmin && (
        <Row>
          <NavBarItem navigateTo="/admin/users" label="Users" claim={Claim.administrator} />
          <NavBarItem
            navigateTo="/admin/data/sources"
            label="Sources"
            claim={Claim.administrator}
          />
          <NavBarItem
            navigateTo="/admin/contents/log"
            label="Linked Snippet Log"
            claim={Claim.administrator}
          />
          <NavBarItem
            navigateTo="/admin/reports/cbra"
            label="CBRA Report"
            claim={Claim.administrator}
          />
        </Row>
      )}
    </NavBarGroup>
  );
};
