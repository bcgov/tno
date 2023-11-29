import { PageSection } from 'components/section';
import { filterFormat } from 'features/search-page/utils';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaPlay, FaRegSmile, FaSearch } from 'react-icons/fa';
import { FaBookmark, FaCloudArrowUp, FaIcons, FaNewspaper, FaTv, FaUsers } from 'react-icons/fa6';
import { IoIosCog, IoMdRefresh } from 'react-icons/io';
import { useNavigate } from 'react-router';
import { useSearchParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useLookup } from 'store/hooks';
import {
  Button,
  Col,
  generateQuery,
  IFilterModel,
  Row,
  Show,
  Text,
  TextArea,
  toQueryString,
} from 'tno-core';

import {
  ContentSection,
  ContributorSection,
  DateSection,
  ExpandableRow,
  MediaSection,
  MediaTypeSection,
  MoreOptions,
  PaperSection,
  SearchInGroup,
  SentimentSection,
  SeriesSection,
} from './components';
import { defaultAdvancedSearch } from './constants';
import { defaultSubMediaGroupExpanded, ISubMediaGroupExpanded } from './interfaces';
import * as styled from './styled';

export interface IAdvancedSearchProps {
  onSearchPage?: boolean;
}

/***
 * AdvancedSearch is a component that displays the advanced search form in the sidebar.
 * @param expanded - determines whether the advanced search form is expanded or not
 * @param setExpanded - function that controls the expanded state of the advanced search form
 */
export const AdvancedSearch: React.FC<IAdvancedSearchProps> = ({ onSearchPage }) => {
  const navigate = useNavigate();
  const [, { addFilter, getFilter, updateFilter }] = useFilters();
  const [{ actions }] = useLookup();
  const [searchParams] = useSearchParams();
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

  const filterId = React.useMemo(() => Number(searchParams.get('modify')), [searchParams]);
  const [searchName, setSearchName] = React.useState<string>('');
  const [viewedFilter, setViewedFilter] = React.useState<IFilterModel>();
  /** controls the sub group states for media sources. i.e) whether Daily Papers is expanded */
  const [mediaGroupExpandedStates, setMediaGroupExpandedStates] =
    React.useState<ISubMediaGroupExpanded>(defaultSubMediaGroupExpanded);

  const saveSearch = React.useCallback(async () => {
    await addFilter({
      name: searchName,
      query: generateQuery(filterFormat(filter, actions)),
      settings: { ...filterFormat(filter, actions) },
      id: 0,
      sortOrder: 0,
      description: '',
      isEnabled: true,
      reports: [],
      folders: [],
    })
      .then((data) => toast.success(`${data.name} has successfully been saved.`))
      .catch();
  }, [filter, searchName, addFilter, actions]);

  const updateSearch = React.useCallback(async () => {
    viewedFilter &&
      (await updateFilter({
        ...viewedFilter,
        query: generateQuery(filterFormat(filter, actions)),
        settings: { ...filterFormat(filter, actions) },
      })
        .then((data) => toast.success(`${data.name} has successfully been updated.`))
        .catch());
  }, [filter, viewedFilter, updateFilter, actions]);

  const handleSearch = async () => {
    navigate(
      `/search?${toQueryString(
        filterFormat(filter, actions),
      )}&name=${searchName}&modify=${filterId}`,
    );
  };

  /** allow user to hit enter while searching */
  const enterPressed = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  /** get viewed filter if in modify mode */
  React.useEffect(() => {
    if (filterId && !viewedFilter) {
      getFilter(filterId)
        .then((data) => {
          setViewedFilter(data);
        })
        .catch();
    }
    // only run when filterId changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterId]);

  return (
    <styled.AdvancedSearch>
      <PageSection
        ignoreLastChildGap
        header={
          <Row className="top-bar">
            <div className="title">{filterId ? 'Modify Search' : 'Advanced Search'}</div>
            <IoMdRefresh
              className="reset"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Reset filters"
              onClick={() => {
                storeFilter({ ...filter, ...defaultAdvancedSearch });
              }}
            />
          </Row>
        }
      >
        <div className="main-search-body">
          {/* CURRENTLY VIEWED SEARCH NAME IF PRESENT */}
          <Show visible={!!filterId}>
            <div className="viewed-name">
              <FaBookmark />
              <div className="filter-name">{viewedFilter?.name}</div>
            </div>
          </Show>
          {/* SEARCH FOR: */}
          <Row className="search-for-row">
            <label className="label">SEARCH FOR: </label>
            <Col className="text-area-container">
              <TextArea
                value={filter?.search}
                className="text-area"
                onKeyDown={enterPressed}
                name="search"
                onChange={(e) => storeFilter({ ...filter, search: e.target.value })}
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
            {/* CONTENT TYPE SECTION */}
            <Col className="expandable-section">
              <ExpandableRow icon={<FaTv />} title="Content Type">
                <ContentSection />
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
          <div className="label">{!filterId ? 'SAVE SEARCH AS: ' : 'UPDATE SEARCH AS: '} </div>
          <Text
            onChange={(e) => {
              setSearchName(e.target.value);
              !!viewedFilter && setViewedFilter({ ...viewedFilter, name: e.target.value });
            }}
            name="searchName"
            value={!!viewedFilter?.name ? viewedFilter.name : searchName}
          />
          <button className="save-cloud">
            <FaCloudArrowUp onClick={() => (!filterId ? saveSearch() : updateSearch())} />
          </button>
          <Button
            onClick={() => {
              handleSearch();
            }}
            className="search-button-expanded"
          >
            Search
            <FaPlay />
          </Button>
        </Row>
      </PageSection>
    </styled.AdvancedSearch>
  );
};
