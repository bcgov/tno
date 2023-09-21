import {
  SidebarMenuItems,
  sidebarMenuItemsArray,
} from 'components/layout/constants/SidebarMenuItems';
import { SearchWithLogout } from 'components/search-with-logout';
import { Commentary } from 'features/commentary';
import { ViewContent } from 'features/content/view-content';
import { FrontPages } from 'features/front-pages';
import { Home } from 'features/home';
import { MyFolders } from 'features/my-folders';
import { MyMinister } from 'features/my-minister/MyMinister';
import { MyReport } from 'features/my-reports';
import { PressGallery } from 'features/press-gallery';
import { MyMinisterSettings } from 'features/settings';
import { TodaysCommentary } from 'features/todays-commentary';
import { TopStories } from 'features/top-stories';
import React from 'react';
import { FaArrowLeft } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Col, Row, Show } from 'tno-core';

import * as styled from './styled';

/**
 * Main landing page for the subscriber app.
 * @returns Component containing the main "box" that will correspond to the selected item in the left navigation bar. Secondary column displaying commentary and front pages.
 */
export const Landing: React.FC = () => {
  const { id } = useParams();
  const [activeItem, setActiveItem] = React.useState<string>(SidebarMenuItems.home.label);
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();

  /* keep active item in sync with url */
  React.useEffect(() => {
    if (id)
      setActiveItem(
        sidebarMenuItemsArray.find((item) => item.path.includes(id ?? ''))?.label ?? 'View',
      );
  }, [id]);

  return (
    <styled.Landing className="main-container">
      <Row>
        <SearchWithLogout />
      </Row>
      <Row className="contents-container">
        <Col className="main-panel">
          <Show visible={activeItem === 'View'}>
            <div className="title view">
              <FaArrowLeft onClick={() => navigate(-1)} />
            </div>
          </Show>
          <Show visible={activeItem !== 'View'}>
            <div className="title">
              {activeItem === SidebarMenuItems.settings.label
                ? 'Settings | My Minister'
                : activeItem}
            </div>
          </Show>
          <div className="content">
            {/* Home is default selected navigation item on login*/}
            <Show visible={activeItem === SidebarMenuItems.home.label}>
              <Home />
            </Show>
            <Show visible={activeItem === 'View'}>
              <ViewContent />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.settings.label}>
              <MyMinisterSettings />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.myMinister.label}>
              <MyMinister />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.todaysCommentary.label}>
              <TodaysCommentary />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.topStories.label}>
              <TopStories />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.myReports.label}>
              <MyReport />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.pressGallery.label}>
              <PressGallery />
            </Show>
            {/* TODO: Create own component when a/c defined for next iteration */}
            <Show visible={activeItem === SidebarMenuItems.mySearches.label}>
              {!!userInfo?.preferences?.searches?.length &&
                userInfo?.preferences.searches.map(
                  (search: { name: string; queryText: string }) => (
                    <p
                      onClick={() => navigate(`/search/${search.queryText}`)}
                      className="search-links"
                      key={search.name}
                    >
                      {search.name}
                    </p>
                  ),
                )}
            </Show>
            <Show visible={activeItem === SidebarMenuItems.folders.label}>
              <MyFolders />
            </Show>
          </div>
        </Col>
        {/* unsure of whether these items will change depedning on selected item */}
        <Col className="right-panel">
          <Commentary />
          <FrontPages />
        </Col>
      </Row>
    </styled.Landing>
  );
};
