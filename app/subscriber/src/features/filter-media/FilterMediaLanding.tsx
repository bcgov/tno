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
  // const [activeSerie, setActiveSerie] = React.useState<ISeriesModel | null>(null);
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
                      seriesIds: mediaGroup.options.map((o) => o.id),
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
                    checked={filter.seriesIds?.length === activeFilter?.options.length}
                    onChange={(e) => {
                      setActiveSource(null);
                      storeFilter({
                        ...filter,
                        seriesIds: e.target.checked ? activeFilter?.options.map((o) => o.id) : [],
                      });
                    }}
                  />
                </Row>
                <div className="scroll-container">
                  {narrowedOptions.map((opt) => {
                    return (
                      <Row
                        key={`${opt.id}|${opt.listOption}`}
                        onClick={() => {
                          setActiveSource(opt);
                          if (opt.listOption === ListOptionName.Source) {
                            storeFilter({ ...filter, seriesIds: [], sourceIds: [opt.id] });
                          } else if (opt.listOption === ListOptionName.Series) {
                            storeFilter({ ...filter, seriesIds: [opt.id], sourceIds: [] });
                          }
                        }}
                        className={`${
                          activeSource?.name === opt.name ? 'active' : 'inactive'
                        } narrowed-option`}
                      >
                        {opt.name}
                        <Checkbox
                          className="opt-chk"
                          checked={filter.seriesIds?.includes(opt.id)}
                          onChange={(e) => {
                            e.target.checked
                              ? storeFilter({
                                  ...filter,
                                  seriesIds: [...(filter.seriesIds ?? []), opt.id],
                                })
                              : storeFilter({
                                  ...filter,
                                  seriesIds: filter.seriesIds?.filter((id) => id !== opt.id),
                                });
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
