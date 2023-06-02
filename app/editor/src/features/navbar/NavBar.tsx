import React from 'react';
import { Claim, NavBarGroup, NavBarItem, Show } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex/row';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const [activeHover, setActiveHover] = React.useState<string>('');

  const hideRef = React.useRef(false);
  const ref = React.useRef<any>();

  React.useEffect(() => {
    document.addEventListener('click', handleClickOutside, false);
    return () => {
      document.removeEventListener('click', handleClickOutside, false);
    };
  });

  const onMouseOver = () => {
    hideRef.current = false;
  };

  const onMouseLeave = () => {
    hideRef.current = true;
    setTimeout(() => {
      if (hideRef.current) setActiveHover('');
    }, 2000);
  };

  const handleClickOutside = (event: { target: any }) => {
    if (ref.current && !ref.current.contains(event.target)) {
      hideRef.current = true;
      setActiveHover('');
    }
  };

  const handleClick = (menu: string = '') => {
    if (activeHover === menu) setActiveHover('');
    else setActiveHover(menu);
  };

  return (
    <div onMouseLeave={onMouseLeave} onMouseOver={onMouseOver} ref={ref}>
      <NavBarGroup className="navbar">
        <Row>
          <div className="editor" onClick={() => handleClick('editor')}>
            <NavBarItem activeHoverTab={activeHover} label="Editor" claim={Claim.editor} />
          </div>
          <div className="admin" onClick={() => handleClick('admin')}>
            <NavBarItem activeHoverTab={activeHover} label="Admin" claim={Claim.administrator} />
          </div>
          <div className="report" onClick={() => handleClick('report')}>
            <NavBarItem activeHoverTab={activeHover} label="Reports" claim={Claim.administrator} />
          </div>
        </Row>
      </NavBarGroup>
      <NavBarGroup hover className="navbar" onClick={() => handleClick()}>
        <Row hidden={!activeHover}>
          {/* Editor */}
          <Show visible={activeHover === 'editor'}>
            <NavBarItem navigateTo="/contents" label="Content" claim={Claim.editor} level={1} />
            <NavBarItem
              navigateTo="/morning/papers"
              label="Morning Papers"
              claim={Claim.editor}
              level={1}
            />
            <NavBarItem
              navigateTo="/storage/locations/1"
              label="File Explorer"
              claim={Claim.editor}
              level={1}
            />
            <NavBarItem navigateTo="/clips" label="Request Clip" claim={Claim.editor} level={1} />
            <NavBarItem
              navigateTo="/work/orders"
              label="Work Orders"
              claim={Claim.editor}
              level={1}
            />
          </Show>

          {/* Admin */}
          <Show visible={activeHover === 'admin'}>
            <NavBarItem
              navigateTo="/admin/topics"
              label="Topics"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/topic-scores"
              label="Topic Scores"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/programs"
              label="Show/Program"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/contributors"
              label="Columnist/Pundit"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/sources"
              label="Sources"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/products"
              label="Products"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/tags"
              label="Tags"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/alerts"
              label="Alerts"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/users"
              label="Users"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/reports"
              label="Reports"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/notifications"
              label="Notifications"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/actions"
              label="Actions"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/licences"
              label="Licences"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/connections"
              label="Connections"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/data/locations"
              label="Data Locations"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/ingests"
              label="Ingest"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/ingest/types"
              label="Ingest Types"
              claim={Claim.administrator}
              level={1}
            />
          </Show>

          {/* Reports */}
          <Show visible={activeHover === 'report'}>
            <NavBarItem
              navigateTo="/reports/cbra"
              label="CBRA Report"
              claim={Claim.administrator}
              level={1}
            />
          </Show>
        </Row>
      </NavBarGroup>
    </div>
  );
};
