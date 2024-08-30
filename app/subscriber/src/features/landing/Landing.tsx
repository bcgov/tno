import { ViewOptions } from 'components/content-list';
import { FilterOptions, FilterOptionsProvider } from 'components/media-type-filters';
import { INavbarOptionItem, NavbarOptions, navbarOptions } from 'components/navbar/NavbarItems';
import { PageSection } from 'components/section';
import { TopDomains } from 'components/top-domains';
import { Commentary } from 'features/commentary';
import { ViewContent } from 'features/content/view-content';
import AVOverviewPreview from 'features/daily-overview/AVOverviewPreview';
import { MediaOverviewIcons } from 'features/daily-overview/MediaOverviewIcons';
import EventOfTheDayPreview from 'features/event-of-the-day/EventOfTheDayPreview';
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
import { useNavigate, useParams } from 'react-router-dom';
import useMobile from 'store/hooks/app/useMobile';
import { Col, IContentModel, Row, Show } from 'tno-core';

import * as styled from './styled';

/**
 * Main landing page for the subscriber app.
 * @returns Component containing the main "box" that will correspond to the selected item in the left navigation bar. Secondary column displaying commentary and front pages.
 */
export const Landing: React.FC = () => {
  const { id, popout } = useParams();
  const [activeItem, setActiveItem] = React.useState<INavbarOptionItem | undefined>(
    NavbarOptions.home,
  );
  const isMobile = useMobile();
  const navigate = useNavigate();
  /* active content will be stored from this context in order to inject into subsequent components */
  const [activeContent, setActiveContent] = React.useState<IContentModel[]>();
  const [isFullScreen, setIsFullScreen] = React.useState(false);
  /* keep active item in sync with url */
  React.useEffect(() => {
    if (id) setActiveItem(navbarOptions.find((item) => item.path.includes(id ?? '')));
  }, [id]);

  const checkFullScreen = React.useCallback((item: INavbarOptionItem | undefined) => {
    return (
      item === NavbarOptions.todaysFrontPages ||
      item === NavbarOptions.topStories ||
      item === NavbarOptions.home ||
      !item
    );
  }, []);

  React.useEffect(() => {
    if (isMobile) {
      const mainElement = document.querySelector('main');
      if (mainElement) {
        mainElement.scrollTo(0, 0);
      }
    }
    // only want to fire when id changes (user navigates to a new section)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  React.useEffect(() => {
    if (checkFullScreen(activeItem)) {
      setIsFullScreen(false);
    } else {
      setIsFullScreen(true);
    }
  }, [activeItem, checkFullScreen]);

  return (
    <styled.Landing className="main-container">
      <FilterOptionsProvider>
        <Show visible={!!popout}>
          <ViewContent activeContent={activeContent} setActiveContent={setActiveContent} />
        </Show>
        <Row className="contents-container">
          <Show visible={!popout}>
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
                  <Show visible={!activeItem && !!activeContent}>
                    <Row>
                      <img
                        className="back-button"
                        src={'/assets/back-button.svg'}
                        alt="Back"
                        data-tooltip-id="back-button"
                        onClick={() => navigate(-1)}
                      />
                      <span className="content-headline">
                        {activeContent && activeContent[0]?.headline}
                      </span>
                    </Row>
                  </Show>
                  {!!activeItem?.reduxFilterStore && (
                    <>
                      <FilterOptions filterStoreName={activeItem?.reduxFilterStore} />
                      <ViewOptions />
                    </>
                  )}
                </>
              }
              className={`main-panel ${isFullScreen ? 'full-screen' : ''}`}
              includeContentActions={!activeItem && !popout}
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
                <Show visible={activeItem === NavbarOptions.eventOfTheDay}>
                  <EventOfTheDayPreview />
                </Show>
                <Show visible={activeItem === NavbarOptions.eveningOverview}>
                  <AVOverviewPreview />
                </Show>
              </div>
            </PageSection>
            {/* unsure of whether these items will change depending on selected item */}
            <Col className="right-panel">
              <Show visible={!isFullScreen && !popout}>
                <Commentary />
                <TopDomains />
              </Show>
              <Show visible={activeItem === NavbarOptions.eveningOverview}>
                <MediaOverviewIcons />
              </Show>
            </Col>
          </Show>
        </Row>
      </FilterOptionsProvider>
    </styled.Landing>
  );
};
