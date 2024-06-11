import React from 'react';
import { Claim, NavBarGroup, NavBarItem, Show } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex/row';

/**
 * The navigation bar that is used throughout the TNO editor application. Add or remove navigation bar items here.
 */
export const NavBar: React.FC = () => {
  const [activeHover, setActiveHover] = React.useState<string>('');
  const [activeSubmenu, setActiveSubmenu] = React.useState<string>('');
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
      if (hideRef.current) {
        setActiveHover('');
        setActiveSubmenu('');
      }
    }, 2000);
  };

  const handleClickOutside = (event: { target: any }) => {
    if (ref.current && !ref.current.contains(event.target)) {
      hideRef.current = true;
      setActiveHover('');
      setActiveSubmenu('');
    }
  };

  const handleFirstLevelClick = (menu: string = '') => {
    setActiveSubmenu('');
    setActiveHover(activeHover === menu ? '' : menu);
  };

  const handleSecondLevelClick = (submenu: string, event: React.MouseEvent) => {
    event.stopPropagation();
    setActiveSubmenu(activeSubmenu === submenu ? '' : submenu);
  };

  return (
    <div onMouseLeave={onMouseLeave} onMouseOver={onMouseOver} ref={ref}>
      <NavBarGroup className="navbar">
        <Row>
          <div className="home" onClick={() => handleFirstLevelClick('')}>
            <NavBarItem navigateTo="/contents" label="Home" claim={Claim.editor} />
          </div>

          <div className="content" onClick={() => handleFirstLevelClick('content')}>
            <NavBarItem activeHoverTab={activeHover} label="Content" claim={Claim.editor} />
          </div>

          <div className="report-building" onClick={() => handleFirstLevelClick('report-building')}>
            <NavBarItem activeHoverTab={activeHover} label="Report Building" claim={Claim.editor} />
          </div>

          <div
            className="content-configuration"
            onClick={() => handleFirstLevelClick('content-configuration')}
          >
            <NavBarItem
              activeHoverTab={activeHover}
              label="Content Configuration"
              claim={Claim.administrator}
            />
          </div>

          <div className="data-import" onClick={() => handleFirstLevelClick('data-import')}>
            <NavBarItem
              activeHoverTab={activeHover}
              label="Data Import"
              claim={Claim.administrator}
            />
          </div>

          <div className="system-settings" onClick={() => handleFirstLevelClick('system-settings')}>
            <NavBarItem
              activeHoverTab={activeHover}
              label="System Settings"
              claim={Claim.administrator}
            />
          </div>
        </Row>
      </NavBarGroup>
      <NavBarGroup hover zIndex={1001} className="navbar" onClick={() => handleFirstLevelClick()}>
        <Row hidden={!activeHover}>
          {/* Content
              Items:
              All Content, Papers, Transcripts Queue

          */}
          <Show visible={activeHover === 'content'}>
            <NavBarItem navigateTo="/contents" label="All Content" claim={Claim.editor} level={1} />
            <NavBarItem navigateTo="/papers" label="Papers" claim={Claim.editor} level={1} />
            <NavBarItem
              navigateTo="/transcriptions"
              label="Transcripts Queue"
              claim={Claim.editor}
              level={1}
            />
          </Show>

          <Show visible={activeHover === 'report-building'}>
            {/* Report Building
              Items:
              Filters
              Folders
              Evening Overview
              Event of the day
              products
              CBRA report
              Report Configuration
            */}
            <NavBarItem
              navigateTo="/admin/filters"
              label="Filters"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/folders"
              label="Folders"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/reports/av/evening-overview"
              label="Evening Overview"
              claim={Claim.editor}
              level={1}
            />
            <div
              className="event-of-the-day"
              onClick={(e) => handleSecondLevelClick('event-of-the-day-details', e)}
            >
              <NavBarItem label="Event of the day ▼" claim={Claim.administrator} level={1} />
            </div>
            <NavBarItem
              navigateTo="/admin/products"
              label="MMI Products"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/reports/cbra"
              label="CBRA Report"
              claim={Claim.administrator}
              level={1}
            />
            <div
              className="report-configuration"
              onClick={(e) => handleSecondLevelClick('report-configuration', e)}
            >
              <NavBarItem label="Report Configuration ▼" claim={Claim.administrator} level={1} />
            </div>
          </Show>

          {/* Content Configuration
              items:
              Tags，Media Types，Columnists & Pundits，Ministers，Shows & Programs

          */}
          <Show visible={activeHover === 'content-configuration'}>
            <NavBarItem
              navigateTo="/admin/tags"
              label="Tags"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/media-types"
              label="Media Types"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/contributors"
              label="Columnists & Pundits"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/ministers"
              label="Ministers"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/programs"
              label="Shows & Programs"
              claim={Claim.administrator}
              level={1}
            />
          </Show>

          {/* Data Import 
             Items:
             Services dashboard, Ingest, Ingest Types, Media sources, Media licencing
          */}
          <Show visible={activeHover === 'data-import'}>
            <NavBarItem
              navigateTo="/admin/dashboard"
              label="Services Dashboard"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/ingests"
              label="Ingested Services"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/ingest/types"
              label="Ingest Types"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/sources"
              label="Media Sources"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/licences"
              label="Media Licences"
              claim={Claim.administrator}
              level={1}
            />
          </Show>

          {/* System Settings 
            Items:
            Manage Users,System Message, Notifications,Actions, Data Locations, Data Connections,System Configuration
          
          */}
          <Show visible={activeHover === 'system-settings'}>
            <NavBarItem
              navigateTo="/admin/users"
              label="Manage Users"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/system-message"
              label="System Message"
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
              navigateTo="/admin/data/locations"
              label="Data Locations"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/connections"
              label="Data Connections"
              claim={Claim.administrator}
              level={1}
            />
            <NavBarItem
              navigateTo="/admin/settings"
              label="System Configuration"
              claim={Claim.administrator}
              level={1}
            />
          </Show>
        </Row>
        <Row hidden={!activeSubmenu}>
          {/* Items:
              Event of the day
              Topics
              Topic Scoring
          
          */}
          <Show visible={activeSubmenu === 'event-of-the-day-details'}>
            <NavBarItem
              navigateTo="/admin/event-of-the-day"
              label={'Event of the day'}
              claim={Claim.administrator}
              level={2}
            />
            <NavBarItem
              navigateTo="/admin/topics"
              label={`Edit Topics`}
              claim={Claim.editor}
              level={2}
            />
            <NavBarItem
              navigateTo="/admin/topic-scores"
              label="Configure Topic Scoring"
              claim={Claim.administrator}
              level={2}
            />
          </Show>
          {/* Items:
              Manage reports
              Evening Overview Templates
          */}
          <Show visible={activeSubmenu === 'report-configuration'}>
            <NavBarItem
              navigateTo="/admin/reports"
              label={`Manage reports`}
              claim={Claim.administrator}
              level={2}
            />
            <NavBarItem
              navigateTo="/admin/av/evening-overview"
              label="Evening Overview Templates"
              claim={Claim.administrator}
              level={2}
            />
          </Show>
        </Row>
      </NavBarGroup>
    </div>
  );
};
