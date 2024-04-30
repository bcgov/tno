import { PageSection } from 'components/section';
import { useElastic } from 'features/my-searches/hooks';
import { useSearchPageContext } from 'features/search-page/SearchPageContext';
import { filterFormat } from 'features/search-page/utils';
import { handleEnterPressed } from 'features/utils';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaCaretSquareDown, FaCheckSquare, FaPlay, FaRegSmile } from 'react-icons/fa';
import { FaSave } from 'react-icons/fa';
import {
  FaBookmark,
  FaDownLeftAndUpRightToCenter,
  FaIcons,
  FaNewspaper,
  FaTag,
  FaTv,
  FaUpRightAndDownLeftFromCenter,
  FaUsers,
} from 'react-icons/fa6';
import { IoIosCog, IoMdRefresh } from 'react-icons/io';
import { useNavigate } from 'react-router';
import { toast } from 'react-toastify';
import { useApp, useContent, useFilters, useLookup } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import {
  Button,
  Claim,
  Col,
  getCookie,
  IFilterModel,
  IFilterSettingsModel,
  Row,
  setCookie,
  Show,
  Text,
  TextArea,
  ToggleGroup,
} from 'tno-core';

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
  ToggleFilterStyleInfo,
} from './components';
import { defaultAdvancedSearch, defaultFilter } from './constants';
import * as styled from './styled';
import { extractTags, removeTags } from './utils';

export interface IAdvancedSearchProps {
  /** Event fires when search button is clicked. */
  onSearch?: (filter: IFilterSettingsModel) => void;
}

/***
 * AdvancedSearch is a component that displays the advanced search form in the sidebar.
 * @param expanded - determines whether the advanced search form is expanded or not
 * @param setExpanded - function that controls the expanded state of the advanced search form
 */
export const AdvancedSearch: React.FC<IAdvancedSearchProps> = ({ onSearch }) => {
  const navigate = useNavigate();
  const [{ myFilters }, { addFilter, updateFilter }] = useFilters();
  const [
    {
      search: { filter: search },
    },
    { storeSearchFilter },
  ] = useContent();

  const genQuery = useElastic();
  const { expanded, setExpanded } = useSearchPageContext();
  const [{ userInfo }] = useApp();
  const [{ filter: activeFilter }, { storeFilter }] = useProfileStore();
  const [originalFilterSettings, setOriginalFilterSettings] =
    React.useState<IFilterSettingsModel>();

  const [searchName, setSearchName] = React.useState<string>(activeFilter?.name ?? '');

  const displayFiltersAsDropdownCookieKey = 'advancedSearch:displayFiltersAsDropdown';
  const [displayFiltersAsDropdown, setDisplayFiltersAsDropdown] = React.useState<boolean>(() => {
    const cookieVal = getCookie(displayFiltersAsDropdownCookieKey);
    return !!cookieVal;
  });
  const handleChangeDisplayFiltersAsDropdown = React.useCallback(
    async (displayFiltersAsDropdown: boolean) => {
      setDisplayFiltersAsDropdown(displayFiltersAsDropdown);
      if (displayFiltersAsDropdown) {
        setCookie(displayFiltersAsDropdownCookieKey, true);
      } else {
        setCookie(displayFiltersAsDropdownCookieKey, false, {
          expires: 'Thu, 01 Jan 1970 00:00:00 UTC',
        });
      }
    },
    [setDisplayFiltersAsDropdown],
  );

  const isAdmin = userInfo?.roles.includes(Claim.administrator);
  const [{ sources, series, mediaTypes }] = useLookup();

  React.useEffect(() => {
    if (activeFilter) {
      setSearchName(activeFilter.name);
      // save default setting that can be reverted to...
      setOriginalFilterSettings(activeFilter.settings);
    } else setSearchName('');
  }, [activeFilter, setOriginalFilterSettings]);

  const saveSearch = React.useCallback(async () => {
    const settings = filterFormat(search);
    const query = genQuery(settings);
    const filter: IFilterModel = activeFilter
      ? { ...activeFilter, query, settings }
      : { ...defaultFilter, name: searchName, query, settings };

    if (!filter.id) {
      if (!searchName) {
        toast.error('Please enter a name for your search.');
        return;
      }
      if (myFilters.some((f) => f.name === searchName)) {
        toast.error('A filter with this name already exists.');
        return;
      }
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
    genQuery,
    activeFilter,
    searchName,
    addFilter,
    storeFilter,
    storeSearchFilter,
    navigate,
    updateFilter,
    myFilters,
  ]);

  return (
    <styled.AdvancedSearch expanded={expanded}>
      <PageSection
        ignoreLastChildGap
        header={
          <Row className="top-bar">
            <div className="title">{activeFilter ? 'Modify Search' : 'Advanced Search'}</div>
            <Row className="tools">
              <div className="back-text" onClick={() => navigate('/search')}>
                Back to basic
              </div>
              {expanded ? (
                <FaDownLeftAndUpRightToCenter
                  className="minimize"
                  onClick={() => setExpanded(false)}
                />
              ) : (
                <FaUpRightAndDownLeftFromCenter
                  className="expand"
                  onClick={() => setExpanded(true)}
                />
              )}
              <IoMdRefresh
                className="reset"
                data-tooltip-id="main-tooltip"
                data-tooltip-content="Reset filters"
                onClick={() => {
                  let resetFilter = activeFilter
                    ? ({
                        ...defaultAdvancedSearch,
                        ...originalFilterSettings,
                      } as IFilterSettingsModel)
                    : {
                        ...search,
                        ...defaultAdvancedSearch,
                        search: search.search,
                      };
                  storeSearchFilter(resetFilter);
                }}
              />
            </Row>
          </Row>
        }
      >
        <Show visible={expanded}>
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
              <label className="label">SEARCH FOR:</label>
              <Col className="text-area-container">
                <TextArea
                  value={search.search}
                  className="text-area"
                  onKeyDown={(e) => handleEnterPressed(e, () => onSearch?.(search), true)}
                  name="search"
                  onChange={(e) => {
                    const { value } = e.target;
                    const tags = extractTags(value);
                    storeSearchFilter({ ...search, search: removeTags(value), tags: tags ?? [] });
                  }}
                />
                <SearchInGroup />
              </Col>
            </Row>
            <div className="search-in-group space-top"></div>
            <Col className="section top-spacer">
              <div className="narrow-filter-header">
                <label>Narrow your results by: </label>
                <div className="toggle-group-container">
                  <ToggleGroup
                    defaultSelected={displayFiltersAsDropdown ? 'sel' : 'chk'}
                    options={[
                      {
                        id: 'chk',
                        label: '',
                        icon: <FaCheckSquare />,
                        onClick: () => handleChangeDisplayFiltersAsDropdown(false),
                      },
                      {
                        id: 'sel',
                        label: '',
                        icon: <FaCaretSquareDown />,
                        onClick: () => handleChangeDisplayFiltersAsDropdown(true),
                      },
                    ]}
                  />
                  <ToggleFilterStyleInfo />
                </div>
              </div>
              {/* DATE RANGE */}
              <Col className={`date-range-group space-top`}>
                <Row className="option-row">
                  <BsCalendarEvent /> Date range
                </Row>
                <DateSection />
              </Col>
              {/* MEDIA SOURCE SECTION */}
              <Col className={`media-group`}>
                <ExpandableRow
                  icon={<BsSun />}
                  title="Media source"
                  hasValues={!!search.sourceIds?.length}
                >
                  <MediaSection
                    displayFiltersAsDropdown={displayFiltersAsDropdown}
                    sources={sources.filter((s) => s.isEnabled)}
                    mediaTypes={mediaTypes.filter((s) => s.isEnabled)}
                    series={series.filter((s) => s.isEnabled)}
                  />
                </ExpandableRow>
              </Col>
              {/* SENTIMENT SECTION */}
              <Col className={`sentiment-group`}>
                <ExpandableRow
                  icon={<FaRegSmile />}
                  title="Sentiment"
                  hasValues={!!search.sentiment?.length}
                >
                  <SentimentSection />
                </ExpandableRow>
              </Col>
              {/* PAPER SECTION */}
              <Col className="paper-attributes">
                <ExpandableRow
                  icon={<FaNewspaper />}
                  title="Paper attributes"
                  hasValues={!!search.section || !!search.page || !!search.edition}
                >
                  <PaperSection />
                </ExpandableRow>
              </Col>
              {/* CONTRIBUTOR SECTION */}
              <Col className="expandable-section">
                <ExpandableRow
                  icon={<FaUsers />}
                  title="Columnists/Anchors"
                  hasValues={!!search.contributorIds?.length}
                >
                  <ContributorSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
                </ExpandableRow>
              </Col>
              {/* MEDIA TYPES SECTION */}
              <Col className="expandable-section">
                <ExpandableRow
                  icon={<FaIcons />}
                  title="Media Types"
                  hasValues={!!search.mediaTypeIds?.length}
                >
                  <MediaTypeSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
                </ExpandableRow>
              </Col>
              {/* SHOW/PROGRAM SECTION */}
              <Col className="expandable-section">
                <ExpandableRow
                  icon={<FaPlay />}
                  title="Show/Program"
                  hasValues={!!search.seriesIds?.length}
                >
                  <SeriesSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
                </ExpandableRow>
              </Col>
              {isAdmin && (
                <>
                  {/* CONTENT TYPE SECTION */}
                  <Col className="expandable-section">
                    <ExpandableRow
                      icon={<FaTv />}
                      title="Content Type"
                      hasValues={!!search.contentTypes?.length}
                    >
                      <ContentTypeSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
                    </ExpandableRow>
                  </Col>
                  {/* TAG SECTION */}
                  <Col className="expandable-section">
                    <ExpandableRow icon={<FaTag />} title="Tags" hasValues={!!search.tags?.length}>
                      <TagSection displayFiltersAsDropdown={displayFiltersAsDropdown} />
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
            <div className="label">
              {!activeFilter ? 'SAVE SEARCH AS: ' : 'UPDATE SEARCH AS: '}{' '}
            </div>
            <Text
              onChange={(e) => {
                setSearchName(e.target.value);
              }}
              name="searchName"
              value={searchName}
            />
            <button className="save-cloud" onClick={() => saveSearch()}>
              <FaSave />
            </button>
            <Button
              onClick={() => {
                onSearch?.(search);
              }}
              className="search-button"
            >
              Search
              <FaPlay />
            </Button>
          </Row>
        </Show>
      </PageSection>
    </styled.AdvancedSearch>
  );
};
