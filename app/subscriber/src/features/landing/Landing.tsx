import { BasicSearch } from 'components/basic-search';
import {
  SidebarMenuItems,
  sidebarMenuItemsArray,
} from 'components/layout/constants/SidebarMenuItems';
import { SearchWithLogout } from 'components/search-with-logout';
import { PageSection } from 'components/section';
import { Commentary } from 'features/commentary';
import { ViewContent } from 'features/content/view-content';
import AVOverviewPreview from 'features/daily-overview/AVOverviewPreview';
import { FilterMedia } from 'features/filter-media';
import { Home } from 'features/home';
import { HomeFilters } from 'features/home/home-filters';
import { MyFolders } from 'features/my-folders';
import { MyMinister } from 'features/my-minister/MyMinister';
import { MyProducts } from 'features/my-products';
import { MySearches } from 'features/my-searches';
import { PressGallery } from 'features/press-gallery';
import { MyMinisterSettings } from 'features/settings';
import { TodaysCommentary } from 'features/todays-commentary';
import { TodaysFrontPages } from 'features/todays-front-pages';
import { TopStories } from 'features/top-stories';
import React from 'react';
import { useParams } from 'react-router-dom';
import { Col, IContentModel, Row, Show } from 'tno-core';

import * as styled from './styled';

/**
 * Main landing page for the subscriber app.
 * @returns Component containing the main "box" that will correspond to the selected item in the left navigation bar. Secondary column displaying commentary and front pages.
 */
export const Landing: React.FC = () => {
  const { id } = useParams();
  const [activeItem, setActiveItem] = React.useState<string>(SidebarMenuItems.home.label);
  /* active content will be stored from this context in order to inject into subsequent components */
  const [activeContent, setActiveContent] = React.useState<IContentModel[]>();
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
        <BasicSearch />
      </Row>
      <Row className="contents-container">
        <PageSection
          ignoreMinWidth
          activeContent={activeContent}
          header={
            <>
              <Show visible={activeItem !== 'View'}>
                {activeItem === SidebarMenuItems.settings.label
                  ? 'Settings | My Minister'
                  : activeItem}
              </Show>
              <Show visible={activeItem === SidebarMenuItems.home.label}>
                <HomeFilters />
              </Show>
            </>
          }
          className="main-panel"
          includeContentActions={activeItem === 'View'}
        >
          <div className="content">
            {/* Home is default selected navigation item on login*/}
            <Show visible={activeItem === SidebarMenuItems.home.label}>
              <Home />
            </Show>
            <Show visible={activeItem === 'View'}>
              <ViewContent setActiveContent={setActiveContent} />
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
            <Show visible={activeItem === SidebarMenuItems.todaysFrontPages.label}>
              <TodaysFrontPages />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.topStories.label}>
              <TopStories />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.myProducts.label}>
              <MyProducts />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.pressGallery.label}>
              <PressGallery />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.filterMedia.label}>
              <FilterMedia />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.mySearches.label}>
              <MySearches />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.folders.label}>
              <MyFolders />
            </Show>
            <Show visible={activeItem === SidebarMenuItems.eveningOverview.label}>
              <AVOverviewPreview />
            </Show>
          </div>
        </PageSection>
        {/* unsure of whether these items will change depending on selected item */}
        <Col className="right-panel">
          <Commentary />
        </Col>
      </Row>
    </styled.Landing>
  );
};
