import { filterFormat } from 'features/search-page/utils';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaPlay, FaRegSmile, FaSearch } from 'react-icons/fa';
import { FaCloudArrowUp, FaIcons, FaNewspaper, FaTags, FaTv, FaUsers } from 'react-icons/fa6';
import { IoIosCog, IoMdRefresh } from 'react-icons/io';
import { useNavigate } from 'react-router';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { Button, Col, generateQuery, Row, Show, Text, TextArea, toQueryString } from 'tno-core';

import {
  ContentSection,
  ContributorSection,
  DateSection,
  ExpandableRow,
  MediaSection,
  MoreOptions,
  PaperSection,
  ProductSection,
  SearchInGroup,
  SentimentSection,
  SeriesSection,
  TagSection,
} from './components';
import { defaultAdvancedSearch } from './constants';
import {
  defaultSubMediaGroupExpanded,
  IAdvancedSearchFilter,
  ISubMediaGroupExpanded,
} from './interfaces';
import * as styled from './styled';
import { queryToState } from './utils/queryToState';

export interface IAdvancedSearchProps {
  expanded: boolean;
  setExpanded?: (expanded: boolean) => void;
  onSearchPage?: boolean;
  setSearchClicked?: (clicked: boolean) => void;
}

/***
 * AdvancedSearch is a component that displays the advanced search form in the sidebar.
 * @param expanded - determines whether the advanced search form is expanded or not
 * @param setExpanded - function that controls the expanded state of the advanced search form
 */
export const AdvancedSearch: React.FC<IAdvancedSearchProps> = ({
  expanded,
  setExpanded,
  onSearchPage,
  setSearchClicked,
}) => {
  const navigate = useNavigate();
  const [, { addFilter }] = useFilters();
  const [{ actions }] = useLookup();
  const [{ filter }, { storeFilter }] = useContent();
  /** controls the sub group states for media sources. i.e) whether Daily Papers is expanded */
  const [mediaGroupExpandedStates, setMediaGroupExpandedStates] =
    React.useState<ISubMediaGroupExpanded>(defaultSubMediaGroupExpanded);
  const [searchName, setSearchName] = React.useState<string>('');

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

  const handleSearch = async () => {
    navigate(`/search`);
    setSearchClicked && setSearchClicked(true);
  };

  /** allow user to hit enter while searching */
  const enterPressed = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      handleSearch();
    }
  };

  return (
    <styled.AdvancedSearch expanded={expanded}>
      <div className="main-search-body">
        <Show visible={expanded}>
          <Row className="top-bar">
            <div className="title">Advanced Search</div>
            <Show visible={!onSearchPage}>
              <p onClick={() => !!setExpanded && setExpanded(false)} className="back-text">
                BACK TO BASIC
              </p>
            </Show>
            <IoMdRefresh
              className="reset"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Reset filters"
              onClick={() => {
                storeFilter({ ...filter, ...defaultAdvancedSearch });
              }}
            />
          </Row>
        </Show>
        <Row className="search-for-row">
          <label className={expanded ? 'label-expanded' : 'label'}>SEARCH FOR: </label>
          <Show visible={!expanded}>
            <Row className="search-bar space-top">
              <FaSearch onClick={() => handleSearch()} className="search-icon" />
              <Text
                className="search-input"
                onKeyDown={enterPressed}
                name="search"
                onChange={(e) => {
                  storeFilter({ ...filter, searchTerm: e.target.value });
                }}
              />
            </Row>
          </Show>
          <Show visible={expanded}>
            <Col className="text-area-container">
              <TextArea
                value={filter?.searchTerm}
                className="text-area"
                onKeyDown={enterPressed}
                name="search"
                onChange={(e) => storeFilter({ ...filter, searchTerm: e.target.value })}
              />
              <SearchInGroup />
            </Col>
          </Show>
          <Show visible={!expanded}>
            <Button
              className="search-button"
              onClick={() => {
                handleSearch();
              }}
            >
              Search <FaPlay />
            </Button>
          </Show>
          <Show visible={!expanded}>
            <p onClick={() => handleSearch()} className="use-text">
              GO ADVANCED
            </p>
          </Show>
        </Row>

        <Show visible={expanded}>
          <div className="search-in-group space-top"></div>
          <Col className="section top-spacer">
            <b>Narrow your results by: </b>
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
            {/* PRODUCT SECTION */}
            <Col className="expandable-section">
              <ExpandableRow icon={<FaIcons />} title="Product">
                <ProductSection />
              </ExpandableRow>
            </Col>
            {/* SHOW/PROGRAM SECTION */}
            <Col className="expandable-section">
              <ExpandableRow icon={<FaPlay />} title="Show/Program">
                <SeriesSection />
              </ExpandableRow>
            </Col>
            {/* TAG SECTION */}
            <Col>
              <ExpandableRow icon={<FaTags />} title="Tags">
                <TagSection />
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
        </Show>
      </div>
      <Show visible={expanded}>
        <Row className="adv-toolbar">
          <div className="label">SAVE SEARCH AS: </div>
          <Text
            onChange={(e) => {
              setSearchName(e.target.value);
            }}
            name="searchName"
          />
          <button className="save-cloud">
            <FaCloudArrowUp onClick={() => saveSearch()} />
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
      </Show>
    </styled.AdvancedSearch>
  );
};
