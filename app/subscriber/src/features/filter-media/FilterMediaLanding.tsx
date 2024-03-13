import { PageSection } from 'components/section';
import { useSubMediaGroups } from 'features/hooks';
import { ISubMediaGroupItem } from 'features/search-page/components/advanced-search/interfaces';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Col, ISeriesModel, ISourceModel, Row, Show } from 'tno-core';

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
  const [narrowedOptions, setNarrowedOptions] = React.useState<ISourceModel[]>([]);
  const [activeSource, setActiveSource] = React.useState<ISourceModel | null>(null);
  // const [activeSerie, setActiveSerie] = React.useState<ISeriesModel | null>(null);
  const [parentClicked, setParentClicked] = React.useState<boolean>(false);

  const [loaded, setLoaded] = React.useState<boolean>(false);

  const determineSourceIds = React.useCallback(
    (mediaGroupFilter?: ISubMediaGroupItem) => {
      if (!mediaGroupFilter) mediaGroupFilter = activeFilter;
      return mediaGroups
        ?.find((sg) => sg.label === mediaGroupFilter?.label)
        ?.options.map((opt) => opt.id);
    },
    [activeFilter, mediaGroups],
  );

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
        const sourceIds = determineSourceIds(dailyPrintMediaGroup);
        storeFilter({ ...filter, sourceIds });
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
      storeFilter({
        ...filter,
        sourceIds: determineSourceIds(),
      });
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
  }, [activeLetter, activeFilter, mediaGroups]);

  return (
    <styled.FilterMediaLanding>
      <Col className="filters">
        <PageSection ignoreLastChildGap header="Filter by Media Type" includeHeaderIcon>
          {/* TODO: Move into reusable component, this type of filter is only used on this page currently*/}
          <Row className="main-media">
            <Col className="media-filter">
              {mediaGroups?.map((mediaGroup) => (
                <div
                  key={`${mediaGroup.key}`}
                  onClick={() => {
                    if (mediaGroup.label === 'Weekly Print') setActiveLetter('A');
                    else if (mediaGroup.label === 'Online') setActiveLetter('B');
                    else setActiveLetter('All');
                    setActiveFilter(mediaGroup);
                    console.log('mediaFilter', mediaGroup);
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
                <div
                  onClick={() => {
                    setActiveSource(null);
                    setNarrowedOptions(activeFilter?.options ?? []);
                  }}
                  className="show-all"
                >
                  Show all
                </div>
                <div className="scroll-container">
                  {narrowedOptions.map((opt) => {
                    return (
                      <div
                        key={`${opt.id}`}
                        onClick={() => {
                          setActiveSource(opt);
                          storeFilter({ ...filter, sourceIds: [opt.id] });
                        }}
                        className={`${
                          activeSource?.name === opt.name ? 'active' : 'inactive'
                        } narrowed-option`}
                      >
                        {`${opt.name}  ${
                          !!opt.shortName && opt.shortName !== opt.name ? `- ${opt.shortName}` : ''
                        }`}
                      </div>
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
