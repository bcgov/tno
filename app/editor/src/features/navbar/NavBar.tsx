import React, { useState } from 'react';
import { Claim, NavBarGroup, NavBarItem, Show } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex/row';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const [activeHover, setActiveHover] = useState<string>('');
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
          <div className="report" onMouseOver={() => setActiveHover('report')}>
            <NavBarItem
              activeHoverTab={activeHover}
              navigateTo="/reports"
              label="Reports"
              claim={Claim.administrator}
            />
          </div>
        </Row>
      </NavBarGroup>
      <NavBarGroup hover className="navbar">
        <Row hidden={!activeHover}>
          {/* Editor */}
          <Show visible={activeHover === 'editor'}>
            <NavBarItem navigateTo="/contents" label="Snippets" claim={Claim.editor} />
            <NavBarItem navigateTo="/storage" label="Storage" claim={Claim.editor} />
            <NavBarItem
              navigateTo="/contents/log"
              label="Linked Snippet Log"
              claim={Claim.administrator}
            />
          </Show>

          {/* Admin */}
          <Show visible={activeHover === 'admin'}>
            <NavBarItem navigateTo="/admin/users" label="Users" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/sources" label="Sources" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/products" label="Products" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/series" label="Series" claim={Claim.administrator} />
            <NavBarItem
              navigateTo="/admin/categories"
              label="Categories"
              claim={Claim.administrator}
            />
            <NavBarItem navigateTo="/admin/tags" label="Tags" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/licenses" label="Licenses" claim={Claim.administrator} />
            <NavBarItem navigateTo="/admin/actions" label="Actions" claim={Claim.administrator} />
            <NavBarItem
              navigateTo="/admin/media/types"
              label="Media Types"
              claim={Claim.administrator}
            />
            <NavBarItem
              navigateTo="/admin/connections"
              label="Connections"
              claim={Claim.administrator}
            />
            <NavBarItem navigateTo="/admin/ingests" label="Ingest" claim={Claim.administrator} />
          </Show>

          {/* Reports */}
          <Show visible={activeHover === 'report'}>
            <NavBarItem
              navigateTo="/reports/cbra"
              label="CBRA Report"
              claim={Claim.administrator}
            />
          </Show>
        </Row>
      </NavBarGroup>
    </div>
  );
};
