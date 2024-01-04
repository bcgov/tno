import { PageSection } from 'components/section';
import { useElastic } from 'features/my-searches/hooks';
import { filterFormat } from 'features/search-page/utils';
import { handleEnterPressed } from 'features/utils';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaPlay, FaRegSmile } from 'react-icons/fa';
import {
  FaBookmark,
  FaCloudArrowUp,
  FaIcons,
  FaNewspaper,
  FaTag,
  FaTv,
  FaUsers,
} from 'react-icons/fa6';
import { IoIosCog, IoMdRefresh } from 'react-icons/io';
import { useNavigate } from 'react-router';
import { toast } from 'react-toastify';
import { useApp, useContent, useFilters, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Button, Claim, Col, IFilterModel, Row, Show, Text, TextArea } from 'tno-core';

import {
  ContentTypeSection,
  ContributorSection,
  DateSection,
  ElasticInfo,
  ExpandableRow,
  MediaSection,
  MediaTypeSection,
  MoreOptions,
  PaperSection,
  SearchInGroup,
  SentimentSection,
  SeriesSection,
  TagSection,
} from './components';
import { defaultAdvancedSearch, defaultFilter } from './constants';
import { defaultSubMediaGroupExpanded, ISubMediaGroupExpanded } from './interfaces';
import * as styled from './styled';

export interface IAdvancedSearchProps {
  /** Event fires when search button is clicked. */
  onSearch?: () => void;
}

/***
 * AdvancedSearch is a component that displays the advanced search form in the sidebar.
 * @param expanded - determines whether the advanced search form is expanded or not
 * @param setExpanded - function that controls the expanded state of the advanced search form
 */
export const AdvancedSearch: React.FC<IAdvancedSearchProps> = ({ onSearch }) => {
  const navigate = useNavigate();
  const [, { addFilter, updateFilter }] = useFilters();
  const [{ actions }] = useLookup();
  const [
    {
      search: { filter: search },
    },
    { storeSearchFilter },
  ] = useContent();
  const genQuery = useElastic();
  const [{ userInfo }] = useApp();
  const [{ filter: activeFilter }, { storeFilter }] = useProfileStore();

  const [searchName, setSearchName] = React.useState<string>(activeFilter?.name ?? '');
  /** controls the sub group states for media sources. i.e) whether Daily Papers is expanded */
  const [mediaGroupExpandedStates, setMediaGroupExpandedStates] =
    React.useState<ISubMediaGroupExpanded>(defaultSubMediaGroupExpanded);

  const isAdmin = userInfo?.roles.includes(Claim.administrator);

  React.useEffect(() => {
    if (activeFilter) setSearchName(activeFilter.name);
    else setSearchName('');
  }, [activeFilter]);

  const saveSearch = React.useCallback(async () => {
    const settings = filterFormat(search, actions);
    const query = genQuery(settings);
    const filter: IFilterModel = activeFilter
      ? { ...activeFilter, name: searchName, settings, query }
      : { ...defaultFilter, name: searchName };

    if (!filter.id) {
      await addFilter(filter)
        .then((data) => {
          toast.success(`${data.name} has successfully been saved.`);
          storeFilter(data);
          storeSearchFilter(data.settings);
          navigate(`/search/advanced/${data.id}`);
        })
        .catch(() => {});
    } else {
      await updateFilter(filter)
        .then((data) => {
          toast.success(`${data.name} has successfully been saved.`);
          storeSearchFilter(data.settings);
        })
        .catch(() => {});
    }
  }, [
    search,
    actions,
    genQuery,
    activeFilter,
    searchName,
    addFilter,
    storeFilter,
    storeSearchFilter,
    navigate,
    updateFilter,
  ]);

  return (
    <styled.AdvancedSearch>
      <PageSection
        ignoreLastChildGap
        header={
          <Row className="top-bar">
            <div className="title">{activeFilter ? 'Modify Search' : 'Advanced Search'}</div>
            <IoMdRefresh
              className="reset"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Reset filters"
              onClick={() => {
                storeSearchFilter({ ...search, ...defaultAdvancedSearch });
              }}
            />
          </Row>
        }
      >
        <div className="main-search-body">
          {/* CURRENTLY VIEWED SEARCH NAME IF PRESENT */}
          <Show visible={!!activeFilter}>
            <div className="viewed-name">
              <FaBookmark />
              <div className="filter-name">{activeFilter?.name}</div>
            </div>
          </Show>
          {/* SEARCH FOR: */}
          <Row className="search-for-row">
            <ElasticInfo />
            <label className="label">SEARCH FOR: </label>
            <Col className="text-area-container">
              <TextArea
                value={search?.search}
                className="text-area"
                onKeyDown={(e) => handleEnterPressed(e, () => onSearch?.(), true)}
                name="search"
                onChange={(e) => storeSearchFilter({ ...search, search: e.target.value })}
              />
              <SearchInGroup />
            </Col>
          </Row>

          <div className="search-in-group space-top"></div>
          <Col className="section top-spacer">
            <b>Narrow your results by: </b>
            {/* DATE RANGE */}
            <Col className={`date-range-group space-top`}>
              <Row className="option-row">
                <BsCalendarEvent /> Date range
              </Row>
              <DateSection />
            </Col>
            {/* MEDIA SOURCE SECTION */}
            <Col className={`media-group`}>
              <ExpandableRow icon={<BsSun />} title="Media source">
                <MediaSection
                  setMediaGroupExpandedStates={setMediaGroupExpandedStates}
                  mediaGroupExpandedStates={mediaGroupExpandedStates}
                />
              </ExpandableRow>
            </Col>
            {/* SENTIMENT SECTION */}
            <Col className={`sentiment-group`}>
              <ExpandableRow icon={<FaRegSmile />} title="Sentiment">
                <SentimentSection />
              </ExpandableRow>
            </Col>
            {/* PAPER SECTION */}
            <Col className="paper-attributes">
              <ExpandableRow icon={<FaNewspaper />} title="Paper attributes">
                <PaperSection />
              </ExpandableRow>
            </Col>
            {/* CONTRIBUTOR SECTION */}
            <Col className="expandable-section">
              <ExpandableRow icon={<FaUsers />} title="Columnists/Anchors">
                <ContributorSection />
              </ExpandableRow>
            </Col>
            {/* MEDIA TYPES SECTION */}
            <Col className="expandable-section">
              <ExpandableRow icon={<FaIcons />} title="Media Types">
                <MediaTypeSection />
              </ExpandableRow>
            </Col>
            {/* SHOW/PROGRAM SECTION */}
            <Col className="expandable-section">
              <ExpandableRow icon={<FaPlay />} title="Show/Program">
                <SeriesSection />
              </ExpandableRow>
            </Col>
            {isAdmin && (
              <>
                {/* CONTENT TYPE SECTION */}
                <Col className="expandable-section">
                  <ExpandableRow icon={<FaTv />} title="Content Type">
                    <ContentTypeSection />
                  </ExpandableRow>
                </Col>
                {/* TAG SECTION */}
                <Col className="expandable-section">
                  <ExpandableRow icon={<FaTag />} title="Tags">
                    <TagSection />
                  </ExpandableRow>
                </Col>
              </>
            )}
          </Col>

          <Row>
            <Col className="section top-spacer">
              <b>Display options:</b>
              {/* SEARCH RESULT SETTINGS SECTION */}
              <ExpandableRow icon={<IoIosCog />} title="Search result options">
                <MoreOptions />
              </ExpandableRow>
            </Col>
          </Row>
        </div>
        {/* FOOTER */}
        <Row className="adv-toolbar">
          <div className="label">{!activeFilter ? 'SAVE SEARCH AS: ' : 'UPDATE SEARCH AS: '} </div>
          <Text
            onChange={(e) => {
              setSearchName(e.target.value);
            }}
            name="searchName"
            value={searchName}
          />
          <button className="save-cloud" onClick={() => saveSearch()}>
            <FaCloudArrowUp />
          </button>
          <Button
            onClick={() => {
              onSearch?.();
            }}
            className="search-button"
          >
            Search
            <FaPlay />
          </Button>
        </Row>
      </PageSection>
    </styled.AdvancedSearch>
  );
};
