import { PageSection } from 'components/section';
import { useElastic } from 'features/my-searches/hooks';
import { useSearchPageContext } from 'features/search-page/SearchPageContext';
import { filterFormat } from 'features/search-page/utils';
import React from 'react';
import { BsCalendarEvent, BsSun } from 'react-icons/bs';
import { FaCaretSquareDown, FaCheckSquare, FaPlay, FaRegSmile } from 'react-icons/fa';
import { FaSave } from 'react-icons/fa';
import {
  FaBookmark,
  FaDownLeftAndUpRightToCenter,
  FaGears,
  FaIcons,
  FaNewspaper,
  FaSquarePlus,
  FaTag,
  FaTv,
  FaUpRightAndDownLeftFromCenter,
  FaUsers,
} from 'react-icons/fa6';
import { IoIosCog, IoMdRefresh } from 'react-icons/io';
import { useNavigate } from 'react-router-dom';
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
  ElasticQueryHelp,
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
  const [searchTerms, setSearchTerms] = React.useState<string>(search?.search ?? '');

  const checkDisplayOptions = React.useCallback((): boolean => {
    return (
      !search.searchUnpublished ||
      search.boldKeywords ||
      (search.actions?.length ?? 0) > 0 ||
      (search.mediaTypeIds?.length ?? 0) > 0 ||
      search.size !== 500
    );
  }, [search]);

  const handleSearch = async () => {
    const updatedFilterSettings = {
      ...search,
      search: searchTerms,
    };

    storeSearchFilter(updatedFilterSettings);

    onSearch?.(updatedFilterSettings);
  };

  React.useEffect(() => {
    if (activeFilter) setSearchName(activeFilter.name);
  }, [activeFilter]);

  React.useEffect(() => {
    if (search) setSearchTerms(search.search ?? '');
  }, [search]);

  React.useEffect(() => {
    if (activeFilter) {
      setSearchName(activeFilter.name);
      // save default setting that can be reverted to...
      setOriginalFilterSettings(activeFilter.settings);
    } else setSearchName('');
  }, [activeFilter, setOriginalFilterSettings]);

  const updateSearch = React.useCallback(
    async (name?: string) => {
      const settings = filterFormat(search);
      const query = genQuery(settings);

      const filter: IFilterModel = {
        ...activeFilter,
        id: activeFilter?.id ?? 0,
        name: name ?? activeFilter?.name ?? '',
        description: activeFilter?.description ?? '',
        sortOrder: activeFilter?.sortOrder ?? 0,
        isEnabled: activeFilter?.isEnabled ?? true,
        query,
        settings,
        reports: activeFilter?.reports ?? [],
        folders: activeFilter?.folders ?? [],
      };

      if (filter.id) {
        if (!filter.name) {
          toast.error('Please enter a name for your search.');
          return;
        }
        await updateFilter(filter)
          .then((data) => {
            toast.success(`${data.name} has successfully been updated.`);
            storeFilter(data);
            storeSearchFilter(data.settings);
          })
          .catch(() => {});
      }
    },
    [search, genQuery, activeFilter, updateFilter, storeFilter, storeSearchFilter],
  );

  const addSearch = React.useCallback(
    async (name: string) => {
      const settings = filterFormat(search);
      const query = genQuery(settings);
      const filter: IFilterModel = { ...defaultFilter, name, query, settings };

      if (!name) {
        toast.error('Please enter a name for your search.');
        return;
      }
      if (myFilters.some((f) => f.name === name)) {
        toast.error('A filter with this name already exists.');
        return;
      }
      await addFilter(filter)
        .then((data) => {
          toast.success(`${data.name} has successfully been created.`);
          storeFilter(data);
          storeSearchFilter(data.settings);
          navigate(`/search/advanced/${data.id}`);
        })
        .catch(() => {});
    },
    [search, genQuery, addFilter, storeFilter, storeSearchFilter, navigate, myFilters],
  );

  const handleSearchTermsChanged = React.useCallback(
    (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      const value = e.target.value;
      setSearchTerms(value);
      storeSearchFilter({ ...search, search: value });
      storeFilter((filter) =>
        filter ? { ...filter, settings: { ...filter?.settings, search: value } } : undefined,
      );
    },
    [search, storeFilter, storeSearchFilter],
  );

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
                <div className="status">
                  {activeFilter?.id && (
                    <button className="status_save_changes" onClick={() => updateSearch()}>
                      Save Changes
                    </button>
                  )}
                </div>
              </div>
            </Show>
            {/* SEARCH FOR: */}
            <Row className="search-for-row">
              <label className="search-in-label">Search for:</label>
              <ElasticQueryHelp queryType={search.queryType} />
              <Col className="text-area-container">
                <TextArea
                  value={searchTerms}
                  className="text-area"
                  name="search"
                  onChange={handleSearchTermsChanged}
                />
                <SearchInGroup />
              </Col>
            </Row>
            <Col className="section">
              <ExpandableRow icon={<FaGears />} title="Advanced Options:" hasValues={false}>
                <Row alignItems="center" justifyContent="space-between">
                  Default operator:
                  <ToggleGroup
                    defaultSelected={search.defaultOperator ?? 'and'}
                    options={[
                      {
                        id: 'and',
                        label: 'AND',
                        onClick: () => storeSearchFilter({ ...search, defaultOperator: 'and' }),
                      },
                      {
                        id: 'or',
                        label: 'OR',
                        onClick: () => storeSearchFilter({ ...search, defaultOperator: 'or' }),
                      },
                    ]}
                  />
                </Row>
                <Row alignItems="center" justifyContent="space-between">
                  Query Type:
                  <ToggleGroup
                    defaultSelected={search.queryType ?? 'simple-query-string'}
                    options={[
                      {
                        id: 'query-string',
                        label: 'Advanced',
                        onClick: () => storeSearchFilter({ ...search, queryType: 'query-string' }),
                      },
                      {
                        id: 'simple-query-string',
                        label: 'Simple',
                        onClick: () =>
                          storeSearchFilter({ ...search, queryType: 'simple-query-string' }),
                      },
                    ]}
                  />
                </Row>
              </ExpandableRow>
            </Col>
            <div className="search-in-group space-top"></div>
            <Col className="section top-spacer">
              <div className="narrow-filter-header">
                <label className="search-in-label">Narrow your results by: </label>
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
                <label className="search-in-label">Display options:</label>
                {/* SEARCH RESULT SETTINGS SECTION */}
                <ExpandableRow
                  icon={<IoIosCog />}
                  title="Search result options"
                  hasValues={checkDisplayOptions()}
                >
                  <MoreOptions />
                </ExpandableRow>
              </Col>
            </Row>
          </div>
          {/* FOOTER */}
          <Row className="adv-toolbar">
            <div className="label">SAVE:</div>
            <Text
              onChange={(e) => {
                setSearchName(e.target.value);
              }}
              name="searchName"
              value={searchName}
            />
            <button
              className="save-cloud"
              onClick={() => (activeFilter?.id ? updateSearch(searchName) : addSearch(searchName))}
              title="Save"
            >
              <FaSave />
            </button>
            {activeFilter?.id && (
              <button className="save-cloud" onClick={() => addSearch(searchName)} title="Add new">
                <FaSquarePlus />
              </button>
            )}
            <Button onClick={handleSearch} className="search-button">
              Search
              <FaPlay />
            </Button>
          </Row>
        </Show>
      </PageSection>
    </styled.AdvancedSearch>
  );
};
