import { ViewOptions } from 'components/content-list';
import { FilterOptions, FilterOptionsProvider } from 'components/media-type-filters';
import { INavbarOptionItem, NavbarOptions, navbarOptions } from 'components/navbar/NavbarItems';
import { PageSection } from 'components/section';
import { TopDomains } from 'components/top-domains';
import { Commentary } from 'features/commentary';
import { ViewContent } from 'features/content/view-content';
import AVOverviewPreview from 'features/daily-overview/AVOverviewPreview';
import { MediaOverviewIcons } from 'features/daily-overview/MediaOverviewIcons';
import { Home } from 'features/home';
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
  const [activeItem, setActiveItem] = React.useState<INavbarOptionItem | undefined>(
    NavbarOptions.home,
  );
  /* active content will be stored from this context in order to inject into subsequent components */
  const [activeContent, setActiveContent] = React.useState<IContentModel[]>();
  /* keep active item in sync with url */
  React.useEffect(() => {
    if (id) setActiveItem(navbarOptions.find((item) => item.path.includes(id ?? '')));
  }, [id]);

  return (
    <styled.Landing className="main-container">
      <FilterOptionsProvider>
        <Row className="contents-container">
          <PageSection
            ignoreMinWidth
            activeContent={activeContent}
            header={
              <>
                <Show visible={!!activeItem}>
                  {activeItem?.icon && <div className="page-icon">{activeItem?.icon}</div>}
                  {activeItem === NavbarOptions.settings
                    ? 'Settings | My Minister'
                    : activeItem?.label}
                </Show>
                {!!activeItem?.reduxFilterStore && (
                  <>
                    <FilterOptions filterStoreName={activeItem?.reduxFilterStore} />
                    <ViewOptions />
                  </>
                )}
              </>
            }
            className="main-panel"
            includeContentActions={!activeItem}
          >
            <div className="content">
              {/* Home is default selected navigation item on login*/}
              <Show visible={activeItem === NavbarOptions.home}>
                <Home />
              </Show>
              <Show visible={!activeItem}>
                <ViewContent setActiveContent={setActiveContent} />
              </Show>
              <Show visible={activeItem === NavbarOptions.settings}>
                <MyMinisterSettings />
              </Show>
              <Show visible={activeItem === NavbarOptions.myMinister}>
                <MyMinister />
              </Show>
              <Show visible={activeItem === NavbarOptions.todaysCommentary}>
                <TodaysCommentary />
              </Show>
              <Show visible={activeItem === NavbarOptions.todaysFrontPages}>
                <TodaysFrontPages />
              </Show>
              <Show visible={activeItem === NavbarOptions.topStories}>
                <TopStories />
              </Show>
              <Show visible={activeItem === NavbarOptions.myProducts}>
                <MyProducts />
              </Show>
              <Show visible={activeItem === NavbarOptions.pressGallery}>
                <PressGallery />
              </Show>
              <Show visible={activeItem === NavbarOptions.mySearches}>
                <MySearches />
              </Show>
              <Show visible={activeItem === NavbarOptions.eveningOverview}>
                <AVOverviewPreview />
              </Show>
            </div>
          </PageSection>
          {/* unsure of whether these items will change depending on selected item */}
          <Col className="right-panel">
            <Show visible={activeItem !== NavbarOptions.eveningOverview}>
              <Commentary />
              <TopDomains />
            </Show>
            <Show visible={activeItem === NavbarOptions.eveningOverview}>
              <MediaOverviewIcons />
            </Show>
          </Col>
        </Row>
      </FilterOptionsProvider>
    </styled.Landing>
  );
};
