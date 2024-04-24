import { PageSection } from 'components/section';
import { useSubMediaGroups } from 'features/hooks';
import { ISubMediaGroupItem } from 'features/search-page/components/advanced-search/interfaces';
import { IGroupOption } from 'features/search-page/components/advanced-search/interfaces/IGroupOption';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Checkbox, Col, ListOptionName, Row, Show } from 'tno-core';

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

      // remove all option from the subMediaGroups
      setMediaGroups(subMediaGroups?.filter((sg) => sg.label !== 'All'));
    }
  }, [subMediaGroups]);

  // init
  React.useEffect(() => {
    if (loaded && mediaGroups && !activeFilter) {
      const dailyPrintMediaGroup = mediaGroups.find((sg) => sg.label === 'Daily Print');
      if (dailyPrintMediaGroup) {
        setActiveFilter(dailyPrintMediaGroup);
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
        if (opt.listOption === ListOptionName.Source) {
          storeFilter({
            ...filter,
            seriesIds: [],
            sourceIds: [...(filter.sourceIds ?? []), opt.id],
          });
        } else if (opt.listOption === ListOptionName.Series) {
          storeFilter({
            ...filter,
            seriesIds: [...(filter.seriesIds ?? []), opt.id],
            sourceIds: [],
          });
        }
      } else {
        // filter out unchecked options depending on the listOption
        if (opt.listOption === ListOptionName.Source) {
          storeFilter({ ...filter, sourceIds: filter.sourceIds?.filter((id) => id !== opt.id) });
        } else if (opt.listOption === ListOptionName.Series) {
          storeFilter({ ...filter, seriesIds: filter.seriesIds?.filter((id) => id !== opt.id) });
        }
      }
    },
    [filter, storeFilter],
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
  }, [activeLetter, activeFilter]);

  // a little bit funky, but needed to keep track if the select all option is checked to maintain checked state
  const allSelected = {
    sourceIds: narrowedOptions
      .filter((opt) => opt.listOption === ListOptionName.Source)
      .map((opt) => opt.id),
    seriesIds: narrowedOptions
      .filter((opt) => opt.listOption === ListOptionName.Series)
      .map((opt) => opt.id),
  };

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
                    if (mediaGroup.label === 'Weekly Print') setActiveLetter('A');
                    else if (mediaGroup.label === 'Online') setActiveLetter('B');
                    else setActiveLetter('All');
                    storeFilter({
                      ...filter,
                      sourceIds: [],
                      seriesIds: [],
                      mediaTypeIds: [mediaGroup.key],
                    });
                    setActiveFilter(mediaGroup);
                    setParentClicked(true);
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
                  Show all
                  <Checkbox
                    className="opt-chk"
                    checked={
                      allSelected.seriesIds.length === filter.seriesIds?.length &&
                      allSelected.sourceIds.length === filter.sourceIds?.length
                    }
                    onChange={(e) => {
                      setActiveSource(null);
                      // if changing to unchecked, remove all sourceIds and seriesIds (toggle)
                      if (!e.target.checked) {
                        storeFilter({
                          ...filter,
                          sourceIds: undefined,
                          seriesIds: undefined,
                        });
                      } else {
                        // need to iterate through and check the options to their corresponding source or series id
                        storeFilter({
                          ...filter,
                          sourceIds: allSelected.sourceIds,
                          seriesIds: allSelected.seriesIds,
                        });
                      }
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
                          checked={
                            filter.seriesIds?.includes(opt.id) || filter.sourceIds?.includes(opt.id)
                          }
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
