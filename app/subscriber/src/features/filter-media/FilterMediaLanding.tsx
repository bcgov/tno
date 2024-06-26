import { PageSection } from 'components/section';
import { useSubMediaGroups } from 'features/hooks';
import { ISubMediaGroupItem } from 'features/search-page/components/advanced-search/interfaces';
import { IGroupOption } from 'features/search-page/components/advanced-search/interfaces/IGroupOption';
import moment from 'moment';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Checkbox, Col, IFilterSettingsModel, ListOptionName, Row, Show } from 'tno-core';

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

  const [loaded, setLoaded] = React.useState<boolean>(false);

  React.useEffect(() => {
    if (subMediaGroups && subMediaGroups.length > 0) {
      setLoaded(true);
      setMediaGroups(subMediaGroups);
    }
  }, [subMediaGroups]);

  // init
  React.useEffect(() => {
    if (loaded && mediaGroups && !activeFilter) {
      const activeSubMediaGroup =
        subMediaGroups.find((sg) => sg.label === filter.activeSubGroup) ??
        subMediaGroups.find((sg) => sg.label === 'Daily Print');
      if (activeSubMediaGroup) {
        setActiveFilter(activeSubMediaGroup);
        setActiveSource(null);
        setNarrowedOptions(activeSubMediaGroup?.options ?? []);
        checkAllOptions(activeSubMediaGroup, true);
        let seriesIds: number[] = [];
        let sourceIds: number[] = [];
        if (activeSubMediaGroup?.listOption === ListOptionName.Series) {
          seriesIds = activeSubMediaGroup.options.map((x) => x.id);
        }
        if (activeSubMediaGroup?.listOption === ListOptionName.Source) {
          sourceIds = activeSubMediaGroup.options.map((x) => x.id);
        }
        const newFilter: IFilterSettingsModel = {
          ...filter,
          startDate: moment(new Date()).startOf('day').toISOString(),
          endDate: moment(new Date()).endOf('day').toISOString(),
          mediaTypeIds: [activeSubMediaGroup.key],
          seriesIds,
          sourceIds,
        };
        storeFilter(newFilter);
      }
    }
    // only when media groups / relevant data is loaded & ready
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loaded]);

  const handleClick = React.useCallback(
    (opt: IGroupOption, checkbox?: HTMLInputElement) => {
      setActiveSource(opt);
      // if checkboxed is checked or if there is no checkbox (meaning the user clicked the row)
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
          sourceIds: sourceIds.length > 0 ? sourceIds : [9999],
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
        activeFilter?.options.filter((opt) => opt.name.startsWith(activeLetter)) ?? [],
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
      let sourceIds = mediaGroup.options
        .filter((x) => x.listOption === ListOptionName.Source && x.selected === true)
        .map((x) => x.id);
      let seriesIds = mediaGroup.options
        .filter((x) => x.listOption === ListOptionName.Series && x.selected === true)
        .map((x) => x.id);
      storeFilter({
        ...filter,
        sourceIds,
        seriesIds,
        mediaTypeIds: [mediaGroup.key],
      });
      setActiveFilter(mediaGroup);
      storeFilter({ ...filter, activeSubGroup: mediaGroup.label });
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
          sourceIds: [],
          seriesIds: [],
        });
      } else {
        // need to iterate through and check the options to their corresponding source or series id
        if (activeFilter) {
          checkAllOptions(activeFilter, true);
        }
        const sourceIds = activeFilter?.options
          .filter((x) => x.listOption === ListOptionName.Source)
          .map((c) => c.id);
        const seriesIds = activeFilter?.options
          .filter((x) => x.listOption === ListOptionName.Series)
          .map((c) => c.id);
        storeFilter({
          ...filter,
          sourceIds,
          seriesIds,
        });
      }
    },
    [activeFilter, filter, storeFilter],
  );

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
                  <label className="all-chk" htmlFor="all-chk">
                    Show all
                  </label>
                  <Checkbox
                    className="opt-chk"
                    id="all-chk"
                    checked={isFullSelected(activeFilter)}
                    onChange={(e) => {
                      handleClickAll(e);
                    }}
                  />
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
                        {opt.name}
                        <Checkbox
                          className="opt-chk"
                          checked={opt.selected}
                          onChange={(e) => {
                            handleClick(opt, e.target);
                          }}
                        />
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
          <FilterMedia loaded={loaded} />
        </PageSection>
      </Col>
    </styled.FilterMediaLanding>
  );
};
