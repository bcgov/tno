import { PageSection } from 'components/section';
import { useSubMediaGroups } from 'features/hooks';
import { ISubMediaGroupItem } from 'features/search-page/components/advanced-search/interfaces';
import { IGroupOption } from 'features/search-page/components/advanced-search/interfaces/IGroupOption';
import moment from 'moment';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import {
  Checkbox,
  Col,
  IContentModel,
  IFilterSettingsModel,
  ListOptionName,
  Row,
  Show,
} from 'tno-core';

import { FilterMedia } from './FilterMedia';
import * as styled from './styled';
import { alphabetArray } from './utils';

/** The FilterMediaLanding Consists of the filter to narrow down the results of media, as well as the results. It is returned in a two column layout*/
export const FilterMediaLanding: React.FC = () => {
  const [
    {
      mediaType: { filter },
    },
    { storeMediaTypeFilter: storeFilter },
  ] = useContent();
  const [{ sources, mediaTypes, series }] = useLookup();
  const { subMediaGroups = [] } = useSubMediaGroups(sources, series, mediaTypes);

  const [mediaGroups, setMediaGroups] = React.useState<ISubMediaGroupItem[]>();
  const [activeFilter, setActiveFilter] = React.useState<ISubMediaGroupItem>();
  const [activeLetter, setActiveLetter] = React.useState<string>('All');
  const [narrowedOptions, setNarrowedOptions] = React.useState<IGroupOption[]>([]);
  const [activeSource, setActiveSource] = React.useState<IGroupOption | null>(null);
  const [parentClicked, setParentClicked] = React.useState<boolean>(false);
  const [stateByDate, setStateByDate] = React.useState<{
    [date: string]: {
      [mediaType: string]: { selected: IContentModel[]; isSelectAllChecked: boolean };
    };
  }>({});
  const [loaded, setLoaded] = React.useState<boolean>(false);

  React.useEffect(() => {
    if (subMediaGroups && subMediaGroups.length > 0) {
      setLoaded(true);
      setMediaGroups(subMediaGroups);
    }
  }, [subMediaGroups]);

  const resetFilters = React.useCallback(
    (resetSubMediaGroup = false) => {
      const activeSubMediaGroup = resetSubMediaGroup
        ? subMediaGroups.find((sg) => sg.label === 'Daily Print')
        : subMediaGroups.find((sg) => sg.label === filter.activeSubGroup) ??
          subMediaGroups.find((sg) => sg.label === 'Daily Print');

      if (activeSubMediaGroup) {
        setActiveFilter(activeSubMediaGroup);
        setActiveSource(null);
        setNarrowedOptions(activeSubMediaGroup?.options ?? []);
        checkAllOptions(activeSubMediaGroup, true);
        const newFilter: IFilterSettingsModel = {
          ...filter,
          startDate: moment(new Date()).startOf('day').toISOString(),
          endDate: moment(new Date()).endOf('day').toISOString(),
          mediaTypeIds: [activeSubMediaGroup.key],
          sort: [{ publishedOn: 'desc' }],
        };
        storeFilter(newFilter);
      }
    },
    [filter, storeFilter, subMediaGroups],
  );

  // init
  React.useEffect(() => {
    if (loaded && mediaGroups && !activeFilter) {
      resetFilters();
    }
    // only when media groups / relevant data is loaded & ready
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loaded]);

  const handleClick = React.useCallback(
    (opt: IGroupOption, checkbox?: HTMLInputElement) => {
      setActiveSource(opt);
      // if checkbox is checked or if there is no checkbox (meaning the user clicked the row)
      if (!!checkbox?.checked || !checkbox) {
        opt.selected = true;
      } else {
        opt.selected = false;
      }

      const sourceIds = activeFilter
        ? activeFilter?.options
            .filter((x) => x.selected && x.listOption === ListOptionName.Source)
            .map((c) => c.id)
        : [];
      const seriesIds = activeFilter
        ? activeFilter?.options
            .filter((x) => x.selected && x.listOption === ListOptionName.Series)
            .map((c) => c.id)
        : [];
      if (opt.listOption === ListOptionName.Source) {
        storeFilter({
          ...filter,
          seriesIds: [],
          sourceIds: sourceIds.length > 0 ? sourceIds : [],
        });
      } else if (opt.listOption === ListOptionName.Series) {
        storeFilter({
          ...filter,
          seriesIds: seriesIds.length > 0 ? seriesIds : [],
          sourceIds: [],
        });
      }
    },
    [activeFilter, filter, storeFilter],
  );

  /** When the parent group is clicked, we want to set the sourceIds to the sources of the parent group
   *   Additionally, we want to deselect the narrowed option
   * */
  React.useEffect(() => {
    if (parentClicked) {
      setActiveSource(null); // deselect the narrowed option
      setParentClicked(false);
    }
    // only want to fire when parentClicked changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [parentClicked]);

  React.useEffect(() => {
    if (activeLetter && activeLetter !== 'All') {
      setNarrowedOptions(
        activeFilter?.options.filter((opt) => opt.name.toUpperCase().startsWith(activeLetter)) ??
          [],
      );
    }
    if (activeLetter === 'All') {
      setNarrowedOptions(activeFilter?.options ?? []);
    }
    // only want to fire when filters change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeLetter, activeFilter, filter]);

  const checkAllOptions = (filter: ISubMediaGroupItem, value: boolean) => {
    filter?.options.forEach((x) => {
      x.selected = value;
    });
  };

  const isFullSelected = (filter: ISubMediaGroupItem | undefined) => {
    if (filter) {
      return filter?.options.filter((x) => x.selected === false).length === 0;
    }
    return false;
  };

  const handleGroupClick = React.useCallback(
    (mediaGroup: ISubMediaGroupItem) => {
      if (mediaGroup.label === 'Weekly Print') setActiveLetter('A');
      else if (mediaGroup.label === 'Online') setActiveLetter('B');
      else setActiveLetter('All');
      checkAllOptions(mediaGroup, true);
      storeFilter({
        ...filter,
        sourceIds: [],
        seriesIds: [],
        startDate: moment(new Date()).startOf('day').toISOString(),
        endDate: moment(new Date()).endOf('day').toISOString(),
        mediaTypeIds: [mediaGroup.key],
        activeSubGroup: mediaGroup.label,
        sort: [{ publishedOn: 'desc' }],
      });
      setActiveFilter(mediaGroup);
      setParentClicked(true);
    },
    [filter, storeFilter],
  );

  const handleClickAll = React.useCallback(
    (e: any) => {
      setActiveSource(null);
      // if changing to unchecked, remove all sourceIds and seriesIds (toggle)
      if (!e.target.checked) {
        if (activeFilter) {
          checkAllOptions(activeFilter, false);
        }
        storeFilter({
          ...filter,
          sourceIds: [], // This behaves the same as as show all...
          seriesIds: [], // This behaves the same as as show all...
        });
      } else {
        // need to iterate through and check the options to their corresponding source or series id
        if (activeFilter) {
          checkAllOptions(activeFilter, true);
        }
        storeFilter({
          ...filter,
          sourceIds: [], // When selecting all we just remove this condition.
          seriesIds: [], // When selecting all we just remove this condition.
        });
      }
    },
    [activeFilter, filter, storeFilter],
  );

  const handleReset = React.useCallback(() => {
    setStateByDate((prevState) => {
      const newState = { ...prevState };
      for (const dateKey in newState) {
        for (const mediaTypeId in newState[dateKey]) {
          newState[dateKey][mediaTypeId] = {
            selected: [],
            isSelectAllChecked: false,
          };
        }
      }
      return newState;
    });
    resetFilters(true);
  }, [resetFilters]);

  return (
    <styled.FilterMediaLanding>
      <Col className="filters">
        <PageSection ignoreLastChildGap header="Filter by Media Type" includeHeaderIcon>
          <Row className="main-media">
            <Col className="media-filter">
              {subMediaGroups?.map((mediaGroup) => (
                <div
                  key={`${mediaGroup.key}`}
                  onClick={() => {
                    handleGroupClick(mediaGroup);
                  }}
                  className={`${
                    activeFilter?.label === mediaGroup.label ? 'active' : 'inactive'
                  } option`}
                >
                  {mediaGroup.label}
                </div>
              ))}
            </Col>
            <Col className="narrowed-options">
              <Show
                visible={activeFilter?.label === 'Online' || activeFilter?.label === 'Weekly Print'}
              >
                <Row className="alpha-filter">
                  {alphabetArray().map((letter) => {
                    return (
                      <div
                        className={`${activeLetter === letter ? 'active' : 'inactive'}-letter`}
                        onClick={() => setActiveLetter(letter)}
                        key={`${letter}`}
                      >
                        {letter}
                      </div>
                    );
                  })}
                </Row>
              </Show>
              <Show visible={activeFilter?.label !== 'Events'}>
                <Row
                  onClick={() => {
                    setActiveSource(null);
                    setNarrowedOptions(activeFilter?.options ?? []);
                  }}
                  className="show-all"
                >
                  <Checkbox
                    className="opt-chk"
                    id="all-chk"
                    checked={isFullSelected(activeFilter)}
                    onChange={(e) => {
                      handleClickAll(e);
                    }}
                  />
                  <label className="all-chk" htmlFor="all-chk">
                    Show all
                  </label>
                </Row>
                <div className="scroll-container">
                  {narrowedOptions.map((opt) => {
                    return (
                      <Row
                        key={`${opt.id}|${opt.listOption}`}
                        onClick={() => handleClick(opt)}
                        className={`${
                          activeSource?.name === opt.name ? 'active' : 'inactive'
                        } narrowed-option`}
                      >
                        <Checkbox
                          className="opt-chk"
                          checked={opt.selected}
                          onChange={(e) => {
                            handleClick(opt, e.target);
                          }}
                        />
                        {opt.name}
                      </Row>
                    );
                  })}
                </div>
              </Show>
              <Show visible={activeFilter?.label === 'Events'}>
                <div className="active narrowed-option">Showing all events</div>
              </Show>
            </Col>
          </Row>
        </PageSection>
      </Col>
      <Col className="results">
        <PageSection>
          <FilterMedia
            loaded={loaded}
            onReset={handleReset}
            setStateByDate={setStateByDate}
            stateByDate={stateByDate}
          />
        </PageSection>
      </Col>
    </styled.FilterMediaLanding>
  );
};
