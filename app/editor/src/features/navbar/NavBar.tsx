import React, { useState } from 'react';
import { Claim, NavBarGroup, NavBarItem, Show } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex/row';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const [activeHover, setActiveHover] = useState<'admin' | 'editor' | ''>('');
  console.log(activeHover);

  return (
    <div onMouseLeave={() => setActiveHover('')}>
      <NavBarGroup className="navbar">
        <Row>
          <div className="editor" onMouseOver={() => setActiveHover('editor')}>
            <NavBarItem
              activeHoverTab={activeHover}
              navigateTo="/contents"
              label="Editor"
              claim={Claim.editor}
            />
          </div>
          <div className="admin" onMouseOver={() => setActiveHover('admin')}>
            <NavBarItem
              activeHoverTab={activeHover}
              navigateTo="/admin"
              label="Admin"
              claim={Claim.administrator}
            />
          </div>
        </Row>
      </NavBarGroup>
      <NavBarGroup hover className="navbar">
        <Row hidden={activeHover === ''}>
          <Show visible={activeHover === 'editor'}>
            <NavBarItem navigateTo="/contents" label="Snippets" claim={Claim.editor} />
            <NavBarItem navigateTo="/storage" label="Storage" claim={Claim.editor} />
          </Show>
          <Show visible={activeHover === 'admin'}>
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
          </Show>
        </Row>
      </NavBarGroup>
    </div>
  );
};
