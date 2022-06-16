import React from 'react';
import { useLocation } from 'react-router-dom';
import { Claim } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex/row';
import { NavBarGroup, NavBarItem } from 'tno-core/dist/components/navbar';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const location = useLocation();

  const showEditor =
    location.pathname.startsWith('/contents') ||
    location.pathname.startsWith('/storage') ||
    location.pathname.startsWith('/combined');
  const showAdmin = location.pathname.startsWith('/admin');
  const [activeHover, setActiveHover] = React.useState(false);
  return (
    <div onMouseOver={() => setActiveHover(true)} onMouseLeave={() => setActiveHover(false)}>
      <NavBarGroup className="navbar">
        <Row>
          <NavBarItem navigateTo="/contents" label="Editor" claim={Claim.editor} />
          <NavBarItem navigateTo="/admin" label="Admin" claim={Claim.administrator} />
        </Row>
      </NavBarGroup>
      <NavBarGroup className="navbar">
        {showEditor && activeHover && (
          <Row>
            <NavBarItem navigateTo="/contents" label="Snippets" claim={Claim.editor} />
            <NavBarItem navigateTo="/storage" label="Storage" claim={Claim.editor} />
          </Row>
        )}
        {showAdmin && activeHover && (
          <Row>
            <NavBarItem navigateTo="/admin/users" label="Users" claim={Claim.administrator} />
            <NavBarItem
              navigateTo="/admin/data/sources"
              label="Sources"
              claim={Claim.administrator}
            />
            <NavBarItem
              navigateTo="/admin/media/types"
              label="Media Types"
              claim={Claim.administrator}
            />
            <NavBarItem
              navigateTo="/admin/reports/cbra"
              label="CBRA Report"
              claim={Claim.administrator}
            />
            <NavBarItem
              navigateTo="/admin/contents/log"
              label="Linked Snippet Log"
              claim={Claim.administrator}
            />
          </Row>
        )}
      </NavBarGroup>
    </div>
  );
};
