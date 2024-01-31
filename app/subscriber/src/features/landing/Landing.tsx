import { ViewOptions } from 'components/content-list';
import { IGroupByState, IToggleStates } from 'components/content-list/interfaces';
import { NavbarOptions, navbarOptions } from 'components/navbar/NavbarItems';
import { PageSection } from 'components/section';
import { Commentary } from 'features/commentary';
import { ViewContent } from 'features/content/view-content';
import AVOverviewPreview from 'features/daily-overview/AVOverviewPreview';
import { MediaOverviewIcons } from 'features/daily-overview/MediaOverviewIcons';
import { Home } from 'features/home';
import { HomeFilters } from 'features/home/home-filters';
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
  const [activeItem, setActiveItem] = React.useState<string>(NavbarOptions.home.label);
  /* active content will be stored from this context in order to inject into subsequent components */
  const [activeContent, setActiveContent] = React.useState<IContentModel[]>();
  /** content view options determine which attributes are displayed in the content list */
  const [contentViewOptions, setContentViewOptions] = React.useState<IToggleStates>({
    section: true,
    sentiment: true,
    checkbox: true,
    date: false,
    teaser: true,
  });
  const [groupBy, setGroupBy] = React.useState<IGroupByState>('source');
  /* keep active item in sync with url */
  React.useEffect(() => {
    if (id)
      setActiveItem(navbarOptions.find((item) => item.path.includes(id ?? ''))?.label ?? 'View');
  }, [id]);

  return (
    <styled.Landing className="main-container">
      <Row className="contents-container">
        <PageSection
          ignoreMinWidth
          activeContent={activeContent}
          header={
            <>
              <Show visible={activeItem !== 'View'}>
                {activeItem === NavbarOptions.settings.label
                  ? 'Settings | My Minister'
                  : activeItem}
              </Show>
              <Show visible={activeItem === NavbarOptions.home.label}>
                <HomeFilters />
                <ViewOptions
                  groupBy={groupBy}
                  setGroupBy={setGroupBy}
                  setViewStates={setContentViewOptions}
                  viewStates={contentViewOptions}
                />
              </Show>
            </>
          }
          className="main-panel"
          includeContentActions={activeItem === 'View'}
        >
          <div className="content">
            {/* Home is default selected navigation item on login*/}
            <Show visible={activeItem === NavbarOptions.home.label}>
              <Home contentViewOptions={contentViewOptions} groupBy={groupBy} />
            </Show>
            <Show visible={activeItem === 'View'}>
              <ViewContent setActiveContent={setActiveContent} />
            </Show>
            <Show visible={activeItem === NavbarOptions.settings.label}>
              <MyMinisterSettings />
            </Show>
            <Show visible={activeItem === NavbarOptions.myMinister.label}>
              <MyMinister />
            </Show>
            <Show visible={activeItem === NavbarOptions.todaysCommentary.label}>
              <TodaysCommentary />
            </Show>
            <Show visible={activeItem === NavbarOptions.todaysFrontPages.label}>
              <TodaysFrontPages />
            </Show>
            <Show visible={activeItem === NavbarOptions.topStories.label}>
              <TopStories />
            </Show>
            <Show visible={activeItem === NavbarOptions.myProducts.label}>
              <MyProducts />
            </Show>
            <Show visible={activeItem === NavbarOptions.pressGallery.label}>
              <PressGallery />
            </Show>
            <Show visible={activeItem === NavbarOptions.mySearches.label}>
              <MySearches />
            </Show>
            <Show visible={activeItem === NavbarOptions.eveningOverview.label}>
              <AVOverviewPreview />
            </Show>
          </div>
        </PageSection>
        {/* unsure of whether these items will change depending on selected item */}
        <Col className="right-panel">
          <Show visible={activeItem !== NavbarOptions.eveningOverview.label}>
            <Commentary />
          </Show>
          <Show visible={activeItem === NavbarOptions.eveningOverview.label}>
            <MediaOverviewIcons />
          </Show>
        </Col>
      </Row>
    </styled.Landing>
  );
};
